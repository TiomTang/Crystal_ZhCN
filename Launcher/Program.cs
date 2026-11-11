using System.Diagnostics;
using Client.Bootstrap;
namespace Launcher;
internal static class Program
{
    public static AMain PForm;
    public static bool Launch;
    // Intentionally exclude URLs, configuration values and exception messages.
    internal static void Trace(string message)
    {
        // A helper copy has an authenticated cleanup allowlist; never add a log there.
        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "helper-owner.json")))
            AMain.SaveError(DateTime.UtcNow.ToString("O") + " pid=" + Environment.ProcessId + " " + message);
    }
    [STAThread] private static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        bool healthProbe = args.Length > 0 && args[0] == "--confirm-update";
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
        Trace("start mode=" + (args.Length == 0 ? "normal" : args[0]));
        try
        {
            if (args.Length == 3 && args[0] == "--apply-transaction") { ApplyProgress.Run(args[1], int.Parse(args[2])); return; }
            if (args.Length == 2 && args[0] == "--recover-update") { ApplyProgress.Run(args[1], 0); return; }
            if (args.Length == 2 && args[0] == "--confirm-update" && Guid.TryParseExact(args[1], "N", out _))
            {
                Settings.Load(); using var form = new AMain();
                string ack = PatchEngine.Target(AppContext.BaseDirectory, UpdateTransaction.DirectoryName + "/health-" + args[1]);
                using var output = new FileStream(ack, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(args[1]); output.Write(data); output.Flush(true); Trace("health-confirmed-without-show"); return;
            }
            if (args.Length != 0) throw new ArgumentException("Unknown launcher arguments");
            using (var lease = new InstallationLease(AppContext.BaseDirectory))
            {
                InstallationLease.EnsureLegacyClientStopped(AppContext.BaseDirectory);
                if (DeferredUpdate.LegacyPending(AppContext.BaseDirectory)) throw new IOException("Legacy .launcher-update/pending.json requires manual recovery; staged files preserved.");
                UpdateTransaction.Recover(AppContext.BaseDirectory);
                Settings.Load();
                Application.Run(PForm = new AMain());
                if (DeferredUpdate.Pending(AppContext.BaseDirectory)) { DeferredUpdate.Schedule(AppContext.BaseDirectory); return; }
            }
            if (Launch) { Settings.ReloadPublished(); Process.Start(Settings.Options.StartInfo(AppContext.BaseDirectory)); }
        }
        catch (Exception ex)
        {
            Trace("fatal type=" + ex.GetType().Name);
            // No source URLs or Basic credentials are persisted in helper evidence.
            try
            {
                if (File.Exists(Path.Combine(AppContext.BaseDirectory, "helper-owner.json")))
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "failure.txt"), DateTime.UtcNow.ToString("O") + " " + ex.GetType().Name + ": transaction failed; backups and recovery helper retained where recovery is unresolved.");
            }
            catch { }
            if (!healthProbe) MessageBox.Show(ex.Message + (args.Length > 0 ? "\nExternal recovery helper: " + Path.Combine(AppContext.BaseDirectory, "Launcher.exe") + " --recover-update <installation-directory>" : ""), "Launcher");
            Environment.ExitCode = 1;
        }
    }
}
