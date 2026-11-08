using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
namespace Launcher;
internal static class HelperCleanup
{
    public static void Mark(string folder)
    {
        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).ToDictionary(p => Path.GetRelativePath(folder, p), p => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))));
        File.WriteAllText(Path.Combine(folder, "helper-owner.json"), JsonSerializer.Serialize(files));
    }
    public static void Schedule()
    {
        string folder = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
        string marker = Path.Combine(folder, "helper-owner.json");
        string boundary = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "CrystalLauncher", "UpdateHelpers"));
        if (!Guid.TryParseExact(Path.GetFileName(folder), "N", out _) || !string.Equals(Path.GetDirectoryName(folder), boundary, StringComparison.OrdinalIgnoreCase) || !File.Exists(marker)) return;
        // Inputs are captured from this process, never paths from the update journal.
        string Quote(string value) => "'" + value.Replace("'", "''") + "'";
        string script = Path.Combine(Path.GetTempPath(), "mir2-clean-" + Guid.NewGuid().ToString("N") + ".ps1");
        string text = "$ErrorActionPreference='Stop'\n" +
            "Wait-Process -Id " + Environment.ProcessId + " -ErrorAction SilentlyContinue\n" +
            "$base=" + Quote(folder) + "; $marker=Join-Path $base 'helper-owner.json'\n" +
            "$expected=" + Quote(File.ReadAllText(marker)) + "\n" +
            "if((Get-Content -LiteralPath $marker -Raw) -ne $expected){throw 'Owner marker changed'}\n" +
            "$files=$expected|ConvertFrom-Json; $dirs=@{}\n" +
            "foreach($prop in $files.PSObject.Properties){\n" +
            " $p=[IO.Path]::GetFullPath((Join-Path $base $prop.Name)); if(!$p.StartsWith($base+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe helper path'}\n" +
            " for($a=$p; $a -and $a.Length -ge $base.Length; $a=[IO.Path]::GetDirectoryName($a)){if((Get-Item -LiteralPath $a).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse helper path'}}\n" +
            " if((Get-FileHash -LiteralPath $p -Algorithm SHA256).Hash -ne $prop.Value){throw 'Helper file changed'}\n" +
            " for($d=[IO.Path]::GetDirectoryName($p);$d.Length -gt $base.Length;$d=[IO.Path]::GetDirectoryName($d)){$dirs[$d]=$true}\n" +
            "}\n" +
            "foreach($prop in $files.PSObject.Properties){[IO.File]::Delete((Join-Path $base $prop.Name))}\n" +
            "$dirs.Keys|Sort-Object Length -Descending|ForEach-Object {[IO.Directory]::Delete($_,$false)}\n" +
            "[IO.File]::Delete($marker); [IO.Directory]::Delete($base,$false); [IO.File]::Delete($PSCommandPath)\n";
        File.WriteAllText(script, text);
        var info = new ProcessStartInfo("powershell.exe") { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Path.GetTempPath() };
        foreach (string a in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script }) info.ArgumentList.Add(a);
        if (Process.Start(info) == null) throw new IOException("Helper cleanup could not start; evidence retained");
    }
}
