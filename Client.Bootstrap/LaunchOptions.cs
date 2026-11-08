using System.Diagnostics;
namespace Client.Bootstrap;
public sealed class LaunchOptions
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Resolution { get; set; }
    public string? DisplayMode { get; set; }
    public bool? FpsCap { get; set; }
    public bool? Topmost { get; set; }
    public bool TestConfig { get; set; }
    public static LaunchOptions Parse(string[] args)
    {
        var o = new LaunchOptions(); var seen = new HashSet<string>();
        for (int i = 0; i < args.Length; i++)
        {
            string key = args[i].ToLowerInvariant();
            if (key == "-tc") key = "--test-config";
            if (!seen.Add(key)) throw new ArgumentException("Duplicate option: " + key);
            if (key == "--test-config") { o.TestConfig = true; continue; }
            if (i + 1 == args.Length) throw new ArgumentException("Missing value: " + key);
            string value = args[++i];
            switch (key)
            {
                case "--host":
                    if (Uri.CheckHostName(value) == UriHostNameType.Unknown || value.Any(char.IsWhiteSpace)) throw new ArgumentException("Invalid host");
                    o.Host = value; break;
                case "--port":
                    if (!int.TryParse(value, out int port) || port < 1 || port > 65535) throw new ArgumentException("Invalid port (1..65535)");
                    o.Port = port; break;
                case "--resolution":
                    if (!ResolutionOptions.All.Any(x => $"{x.Width}x{x.Height}" == value)) throw new ArgumentException("Unsupported resolution: " + value);
                    o.Resolution = value; break;
                case "--display-mode":
                    if (value != "windowed" && value != "maximized" && value != "fullscreen") throw new ArgumentException("Invalid display-mode");
                    o.DisplayMode = value; break;
                case "--fps-cap": o.FpsCap = Boolean(value, key); break;
                case "--topmost": o.Topmost = Boolean(value, key); break;
                default: throw new ArgumentException("Unknown option: " + key);
            }
        }
        return o;
    }
    private static bool Boolean(string value, string key) => bool.TryParse(value, out bool result) ? result : throw new ArgumentException("Invalid boolean: " + key);
    public string[] Arguments()
    {
        var a = new List<string>();
        void Add(string key, object? value) { if (value != null) { a.Add(key); a.Add(value.ToString()!); } }
        Add("--host", Host); Add("--port", Port); Add("--resolution", Resolution); Add("--display-mode", DisplayMode); Add("--fps-cap", FpsCap); Add("--topmost", Topmost);
        if (TestConfig) a.Add("--test-config");
        Parse(a.ToArray()); return a.ToArray();
    }
    public ProcessStartInfo StartInfo(string directory)
    {
        string root = Path.GetFullPath(directory);
        var info = new ProcessStartInfo(Path.Combine(root, "Client.exe")) { WorkingDirectory = root, UseShellExecute = false };
        foreach (string arg in Arguments()) info.ArgumentList.Add(arg);
        return info;
    }
}
public sealed class InstallationLease : IDisposable
{
    private readonly FileStream stream;
    public InstallationLease(string directory, bool game = false)
    {
        // Shared by client and launcher: held for the entire game/update lifetime.
        string root = Path.GetFullPath(directory);
        foreach (string path in new[] { root, Path.Combine(root, ".mir2-install.lock") })
        {
            try { if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new IOException("Reparse installation lease path"); }
            catch (FileNotFoundException) { }
        }
        stream = new FileStream(Path.Combine(Path.GetFullPath(directory), ".mir2-install.lock"), FileMode.OpenOrCreate, game ? FileAccess.Read : FileAccess.ReadWrite, game ? FileShare.Read : FileShare.None);
        if (game && (Directory.Exists(Path.Combine(Path.GetFullPath(directory), ".mir2-transaction")) || File.Exists(Path.Combine(Path.GetFullPath(directory), ".launcher-update", "pending.json"))))
        {
            stream.Dispose();
            throw new IOException("An interrupted update requires launcher recovery before the game can start.");
        }
    }
    public static void EnsureLegacyClientStopped(string directory)
    {
        string target = Path.Combine(Path.GetFullPath(directory), "Client.exe");
        foreach (var process in Process.GetProcessesByName("Client"))
        {
            using (process)
            {
                string? path;
                try { path = process.MainModule?.FileName; }
                catch (Exception ex) { throw new IOException("Cannot verify running client; close it before updating.", ex); }
                if (string.Equals(path, target, StringComparison.OrdinalIgnoreCase)) throw new IOException("Close the running game before updating.");
            }
        }
    }
    public void Dispose() => stream.Dispose();
}
