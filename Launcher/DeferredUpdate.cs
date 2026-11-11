using System.Diagnostics;
using Client.Bootstrap;
namespace Launcher;
public enum ApplyPhase { Waiting, Applying, Verifying, Restarting }
public static class DeferredUpdate
{
    // Only known resource trees and the operator INI can be replaced while
    // this process holds launcher dependencies open. All other paths stay deferred.
    public static bool RequiresRestart(IEnumerable<FileInformation> files) => files.Any(file =>
    {
        string name = file.FileName.Replace((char)92, '/');
        return !name.Equals("Launcher.ini", StringComparison.OrdinalIgnoreCase) &&
            !new[] { "Data/", "Map/", "Sound/", "Localization/" }.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    });
    public static bool LegacyPending(string root) => File.Exists(PatchEngine.Target(root, ".launcher-update/pending.json"));
    public static bool Pending(string root) => UpdateTransaction.State(root) == "Prepared";
    public static void Schedule(string root)
    {
        string helper = Path.Combine(Path.GetTempPath(), "CrystalLauncher", "UpdateHelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(helper);
        foreach (string file in Directory.GetFiles(AppContext.BaseDirectory))
        {
            string name = Path.GetFileName(file);
            if (new[] { "Launcher.exe", "Launcher.dll", "Launcher.pdb", "Launcher.deps.json", "Launcher.runtimeconfig.json" }.Contains(name, StringComparer.OrdinalIgnoreCase) || name == "Shared.dll" || name == "Client.Bootstrap.dll" || name.StartsWith("Microsoft.Web.WebView2.", StringComparison.OrdinalIgnoreCase)) File.Copy(file, Path.Combine(helper, name));
        }
        foreach (string name in new[] { "runtimes", "Localization" })
        {
            string source = Path.Combine(AppContext.BaseDirectory, name);
            if (Directory.Exists(source)) CopyTree(source, Path.Combine(helper, name));
        }
        HelperCleanup.Mark(helper);
        File.WriteAllText(Path.Combine(root, UpdateTransaction.DirectoryName, "RECOVERY.txt"), "Run external recovery helper: " + Path.Combine(helper, "Launcher.exe") + " --recover-update <installation-directory>. Keep backups until recovery succeeds.");
        var info = new ProcessStartInfo(Path.Combine(helper, "Launcher.exe")) { WorkingDirectory = helper, UseShellExecute = false };
        info.ArgumentList.Add("--apply-transaction"); info.ArgumentList.Add(Path.GetFullPath(root)); info.ArgumentList.Add(Environment.ProcessId.ToString());
        if (Process.Start(info) == null) throw new IOException("Could not start external recovery helper");
        Program.Trace("self-update-helper-scheduled");
    }
    static void CopyTree(string source, string target)
    {
        if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0) throw new IOException("Reparse helper dependency");
        Directory.CreateDirectory(target);
        foreach (string f in Directory.GetFiles(source))
        { if ((File.GetAttributes(f) & FileAttributes.ReparsePoint) != 0) throw new IOException("Reparse helper dependency"); File.Copy(f, Path.Combine(target, Path.GetFileName(f))); }
        foreach (string d in Directory.GetDirectories(source)) CopyTree(d, Path.Combine(target, Path.GetFileName(d)));
    }
    public static void Apply(string root, int parentId, bool restart = true, Action<ApplyPhase, int, int, string> report = null)
    {
        if (parentId != 0)
        {
            report?.Invoke(ApplyPhase.Waiting, 0, 0, null);
            try { using var parent = Process.GetProcessById(parentId); if (!parent.WaitForExit(30000)) throw new IOException("Launcher did not exit"); }
            catch (ArgumentException) { }
        }
        using (var lease = new InstallationLease(root))
        {
            InstallationLease.EnsureLegacyClientStopped(root);
            if (LegacyPending(root)) throw new IOException("Legacy .launcher-update/pending.json requires manual recovery; files preserved");
            if (parentId == 0) UpdateTransaction.Recover(root);
            else UpdateTransaction.ApplyConfirmed(root,
                token => { report?.Invoke(ApplyPhase.Verifying, 0, 0, null); return Probe(root, token); },
                (done, total, name) => report?.Invoke(ApplyPhase.Applying, done, total, name));
        }
        HelperCleanup.Schedule();
        if (restart) { report?.Invoke(ApplyPhase.Restarting, 0, 0, null); Restart(root); }
    }
    public static void Restart(string root)
    {
        if (Process.Start(new ProcessStartInfo(Path.Combine(Path.GetFullPath(root), "Launcher.exe")) { WorkingDirectory = Path.GetFullPath(root), UseShellExecute = false }) == null) throw new IOException("Committed update but restart failed");
    }
    internal static bool Probe(string root, string token)
    {
        var info = new ProcessStartInfo(Path.Combine(root, "Launcher.exe")) { WorkingDirectory = root, UseShellExecute = false };
        info.ArgumentList.Add("--confirm-update"); info.ArgumentList.Add(token);
        using var process = Process.Start(info);
        if (process == null) return false;
        if (!process.WaitForExit(30000)) { process.Kill(); process.WaitForExit(); return false; }
        string ack = PatchEngine.Target(root, UpdateTransaction.DirectoryName + "/health-" + token);
        bool ok = process.ExitCode == 0 && File.Exists(ack) && File.ReadAllText(ack) == token;
        if (File.Exists(ack)) File.Delete(ack);
        return ok;
    }
}
