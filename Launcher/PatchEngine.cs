using System.IO.Compression;
using System.Diagnostics;
namespace Launcher;
public sealed record PatchSource(string Host, string ListName, bool NeedsLogin, string Login, string Password)
{
    public HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        if (NeedsLogin) client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(Login + ":" + Password)));
        return client;
    }
}
public sealed record PatchPlan(List<FileInformation> Downloads, double MetadataMilliseconds, double CompareMilliseconds, int Directories);
public static class PatchEngine
{
    public static List<FileInformation> ReadList(string host, string name) => ReadList(Settings.Snapshot() with { Host = host, ListName = name });
    public static List<FileInformation> ReadList(PatchSource source)
    {
        using var client = source.CreateClient();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var response = client.GetAsync(source.Host + Path.ChangeExtension(source.ListName, ".gz"), HttpCompletionOption.ResponseHeadersRead, timeout.Token).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        const int max = 64 * 1024 * 1024;
        if (response.Content.Headers.ContentLength > max) throw new InvalidDataException("Patch manifest too large");
        using var input = response.Content.ReadAsStream(timeout.Token); using var output = new MemoryStream();
        byte[] buffer = new byte[65536]; int n;
        while ((n = input.ReadAsync(buffer, timeout.Token).AsTask().GetAwaiter().GetResult()) != 0)
        { if (output.Length + n > max) throw new InvalidDataException("Patch manifest too large"); output.Write(buffer, 0, n); }
        return ParseList(output.ToArray());
    }
    public static List<FileInformation> ParseList(byte[] bytes)
    {
        using var reader = new BinaryReader(new MemoryStream(bytes));
        int count = reader.ReadInt32();
        if (count < 1 || count > 1000000) throw new InvalidDataException("Invalid patch file count");
        var files = new List<FileInformation>(); var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < count; i++)
        {
            var file = new FileInformation(reader);
            ValidateManifestName(file.FileName);
            string canonical = LexicalTarget(AppContext.BaseDirectory, file.FileName);
            if (file.Length < 0 || file.Compressed < 0 || !seen.Add(canonical)) throw new InvalidDataException("Invalid/duplicate patch entry");
            files.Add(file);
        }
        if (reader.BaseStream.Position != reader.BaseStream.Length) throw new InvalidDataException("Trailing patch data");
        return files;
    }
    public static string LexicalTarget(string root, string name)
    {
        string relative = name.Replace('\\', '/');
        if (Path.IsPathRooted(relative) || relative.Contains(':') || relative.Split('/').Any(p => p == ".." || p == "." || p.Length == 0)) throw new InvalidDataException("Unsafe patch path: " + name);
        foreach (string part in relative.Split('/'))
        {
            if (part.EndsWith(' ') || part.EndsWith('.') || part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidDataException("Invalid Windows patch path: " + name);
            string stem = part.Split('.')[0];
            if (new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" }.Contains(stem, StringComparer.OrdinalIgnoreCase)) throw new InvalidDataException("Reserved Windows patch path");
        }
        string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string target = Path.GetFullPath(Path.Combine(prefix, relative));
        if (!target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Patch path escapes installation");
        return target;
    }
    private static bool Attributes(string path, out FileAttributes attributes)
    {
        try { attributes = File.GetAttributes(path); return true; }
        catch (FileNotFoundException) { attributes = default; return false; }
        catch (DirectoryNotFoundException) { attributes = default; return false; }
    }
    private static void ValidatePath(string root, string target, HashSet<string> checkedPaths = null)
    {
        string boundary = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        for (string path = target; path != null && path.Length >= boundary.Length; path = Path.GetDirectoryName(path))
        {
            if (checkedPaths != null && !checkedPaths.Add(path)) break;
            if (Attributes(path, out var attributes) && (attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse point in patch path");
        }
    }
    public static string Target(string root, string name)
    {
        string target = LexicalTarget(root, name);
        ValidatePath(root, target);
        return target;
    }
    public static PatchPlan Check(string root, IReadOnlyList<FileInformation> files, bool force = false)
    {
        // Group only manifest directories. Enumeration includes hidden/system files,
        // never descends into unrelated directories and never follows reparse points.
        var groups = new Dictionary<string, List<(FileInformation Info, string Path)>>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var info in files)
        {
            ValidateManifestName(info.FileName);
            string target = LexicalTarget(root, info.FileName);
            if (info.Length < 0 || info.Compressed < 0 || !seen.Add(target)) throw new InvalidDataException("Invalid/duplicate patch entry");
            if (Protected(info.FileName)) continue;
            string directory = Path.GetDirectoryName(target);
            if (!groups.TryGetValue(directory, out var entries)) groups.Add(directory, entries = new());
            entries.Add((info, target));
        }
        var watch = Stopwatch.StartNew();
        var checkedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var metadata = new Dictionary<string, (long Length, DateTime Time)>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in groups)
        {
            ValidatePath(root, group.Key, checkedPaths);
            if (!Attributes(group.Key, out var attributes)) continue;
            if ((attributes & FileAttributes.Directory) == 0) throw new IOException("Patch directory is a file: " + group.Key);
            var wanted = new HashSet<string>(group.Value.Select(x => x.Path), StringComparer.OrdinalIgnoreCase);
            foreach (var file in new DirectoryInfo(group.Key).EnumerateFiles("*", new EnumerationOptions { RecurseSubdirectories = false, AttributesToSkip = 0, IgnoreInaccessible = false }))
            {
                if (!wanted.Contains(file.FullName)) continue;
                if ((file.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse patch target");
                metadata.Add(file.FullName, (file.Length, file.LastWriteTime));
            }
        }
        double enumeration = watch.Elapsed.TotalMilliseconds;
        watch.Restart();
        var downloads = new List<FileInformation>();
        foreach (var group in groups.Values)
            foreach (var entry in group)
                if (force || !metadata.TryGetValue(entry.Path, out var local) || local.Length != entry.Info.Length || local.Time != entry.Info.Creation) downloads.Add(entry.Info);
        return new PatchPlan(downloads, enumeration, watch.Elapsed.TotalMilliseconds, groups.Count);
    }
    public static void ValidateManifestName(string name)
    {
        if (name.Replace('\\', '/').Split('/').Any(p => p.StartsWith(".mir2-", StringComparison.OrdinalIgnoreCase) || p.Equals(".launcher-update", StringComparison.OrdinalIgnoreCase))) throw new InvalidDataException("Manifest targets updater internals");
    }
    public static bool Protected(string name)
    {
        string path = name.Replace('\\', '/'); string file = Path.GetFileName(path);
        return !path.Equals("Launcher.ini", StringComparison.OrdinalIgnoreCase) && (file.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || file.Equals("Error.txt", StringComparison.OrdinalIgnoreCase) || path.Split('/').Any(p => p.StartsWith(".mir2-", StringComparison.OrdinalIgnoreCase)) || path.Split('/').Any(p => p.Equals(".launcher-update", StringComparison.OrdinalIgnoreCase)) || path.Split('/').Any(p => new[] { "Screenshots", "Logs", "ChatLogs", "UserData" }.Contains(p, StringComparer.OrdinalIgnoreCase)));
    }
    public static string Url(string host, FileInformation info)
    {
        string name = info.FileName.Replace('\\', '/');
        if (name != "PList.gz" && (info.Compressed != info.Length || info.Compressed == 0)) name += ".gz";
        return host + name;
    }
    public static void Apply(string host, string root, FileInformation info)
    {
        string path = Target(root, info.FileName);
        if (Protected(info.FileName)) return;
        var file = new FileInfo(path);
        if (file.Exists && file.Length == info.Length && file.LastWriteTime == info.Creation) return;
        Download(Settings.Snapshot() with { Host = host }, root, info);
    }
    public static void Download(PatchSource source, string root, FileInformation info)
    {
        UpdateTransaction.Prepare(source, root, new[] { info }, CancellationToken.None);
    }
}
