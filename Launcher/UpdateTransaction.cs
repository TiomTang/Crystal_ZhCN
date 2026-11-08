using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Runtime.InteropServices;
using Client.Bootstrap;

namespace Launcher;

// Hashes identify locally staged/backup bytes only; the legacy server manifest has no hash.
public static class UpdateTransaction
{
    public const string DirectoryName = ".mir2-transaction";
    public sealed class Entry
    {
        public string Name { get; set; }
        public long Length { get; set; }
        public long Time { get; set; }
        public string Hash { get; set; }
        public bool Existed { get; set; }
        public string OldHash { get; set; }
        public long OldTime { get; set; }
        public bool Intent { get; set; }
        public bool Restored { get; set; }
        public bool RestorePending { get; set; }
    }
    public sealed class Journal
    {
        public int Version { get; set; } = 1;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string State { get; set; } = "Downloading";
        public List<Entry> Entries { get; set; } = new();
        public string HealthToken { get; set; }
    }
    sealed class Envelope { public string State { get; set; } public byte[] Payload { get; set; } }
    static string Folder(string root) => PatchEngine.Target(root, DirectoryName);
    static string Internal(string root, string name) => PatchEngine.Target(root, DirectoryName + "/" + name);
    static string Slot(string root, int i, bool backup = false) => Internal(root, (backup ? "backup-" : "stage-") + i);
    public static bool Exists(string root) => Directory.Exists(Folder(root));
    public static string State(string root) => Exists(root) ? Load(root).State : null;
    static string Hash(string path) { using var s = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(s)); }
    static bool Matches(string path, string hash) => File.Exists(path) && Hash(path) == hash;
    static void CopyDurable(string source, string destination)
    {
        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
#if TRANSACTION_TESTS
        byte[] buffer = new byte[65536]; int n;
        if (Path.GetFileName(destination).StartsWith("restore-"))
        {
            n = input.Read(buffer, 0, 4); output.Write(buffer, 0, n);
            output.Flush(true); Boundary("rollback-copy-partial");
        }
        input.CopyTo(output);
#else
        input.CopyTo(output);
#endif
        output.Flush(true);
    }
    static void Save(string root, Journal journal, string folderName = DirectoryName)
    {
        string temp = PatchEngine.Target(root, folderName + "/journal.next"), target = PatchEngine.Target(root, folderName + "/journal.json");
        if (File.Exists(temp)) File.Delete(temp);
        using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            var envelope = new Envelope { State = journal.State, Payload = JournalProtection.Transform(JsonSerializer.SerializeToUtf8Bytes(journal), root, true) };
#if TRANSACTION_TESTS
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(envelope);
            if (folderName != DirectoryName)
            {
                Boundary("initial-next-empty");
                stream.Write(bytes, 0, bytes.Length / 2); stream.Flush(true); Boundary("initial-next-partial");
                stream.Write(bytes, bytes.Length / 2, bytes.Length - bytes.Length / 2);
            }
            else
#endif
                JsonSerializer.Serialize(stream, envelope);
            stream.Flush(true);
        }
        if (folderName != DirectoryName) Boundary("initial-next-complete");
        if (File.Exists(target)) File.Replace(temp, target, null); else File.Move(temp, target);
        if (folderName != DirectoryName) Boundary("initial-journal-ready");
    }
    static Journal Load(string root, string folderName = DirectoryName)
    {
        string path = PatchEngine.Target(root, folderName + "/journal.json");
        if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new InvalidDataException("Oversized update journal");
        var envelope = JsonSerializer.Deserialize<Envelope>(File.ReadAllBytes(path), new JsonSerializerOptions { UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow });
        if (envelope?.Payload == null) throw new InvalidDataException("Invalid journal envelope");
        var j = JsonSerializer.Deserialize<Journal>(JournalProtection.Transform(envelope.Payload, root, false));
        if (j?.State != envelope.State || (j.HealthToken != null && !Guid.TryParseExact(j.HealthToken, "N", out _))) throw new InvalidDataException("Edited journal state");
        if (j == null || j.Version != 1 || !Guid.TryParseExact(j.Id, "N", out _) || j.Entries == null || j.Entries.Count > 1000000 ||
            !new[] { "Downloading", "Prepared", "Applying", "AwaitingHealth", "Committed", "RollingBack", "RolledBack" }.Contains(j.State)) throw new InvalidDataException("Invalid update journal; retain recovery evidence");
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in j.Entries)
        {
            if (e == null || e.Name == null || PatchEngine.Protected(e.Name) || !seen.Add(PatchEngine.Target(root, e.Name)) || e.Length < 0 ||
                !ValidHash(e.Hash) || (e.Existed && !ValidHash(e.OldHash)) || (!e.Existed && e.OldHash != null) || (e.Restored && !e.Intent) || (e.RestorePending && (!e.Intent || !e.Existed))) throw new InvalidDataException("Unsafe update journal");
            DateTime.FromBinary(e.Time); if (e.Existed) DateTime.FromBinary(e.OldTime);
        }
        return j;
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool GetDiskFreeSpaceEx(string directory, out ulong available, out ulong total, out ulong free);
    static void CheckDiskBudget(string root, IReadOnlyList<FileInformation> files)
    {
        ulong required = 16 * 1024 * 1024;
        foreach (var file in files)
        {
            PatchEngine.ValidateManifestName(file.FileName);
            if (PatchEngine.Protected(file.FileName)) continue;
            if (file.Length < 0 || file.Compressed < 0) throw new InvalidDataException("Invalid patch length");
            string target = PatchEngine.Target(root, file.FileName);
            checked { required += (ulong)file.Length; if (File.Exists(target)) required += (ulong)new FileInfo(target).Length; }
        }
        if (!GetDiskFreeSpaceEx(Path.GetFullPath(root), out ulong available, out _, out _)) throw new IOException("Cannot verify update disk space");
        if (available < required) throw new IOException("Insufficient disk space for staged update and rollback backups");
    }
    static bool ValidHash(string value) => value != null && value.Length == 64 && value.All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F');
    public static void Boundary(string name)
    {
#if TRANSACTION_TESTS
        // Test builds only: fixed fault/pause probes, never execute supplied commands.
        if (Environment.GetEnvironmentVariable("MIR2_TRANSACTION_PAUSE") == "throw:" + name) throw new IOException("Injected transaction boundary failure: " + name);
        if (Environment.GetEnvironmentVariable("MIR2_TRANSACTION_PAUSE") == name)
        {
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "transaction-paused"), name);
            Thread.Sleep(Timeout.Infinite);
        }
#endif
    }
    public static void Prepare(PatchSource source, string root, IReadOnlyList<FileInformation> files, CancellationToken cancel, Action<string, long, long> progress = null)
    {
        RecoverCompleted(root);
        if (Exists(root)) throw new IOException("Previous transaction requires recovery");
        if (DeferredUpdate.LegacyPending(root)) throw new IOException("Legacy pending update requires manual recovery; .launcher-update is preserved");
        CheckDiskBudget(root, files);
        // Publish only a complete initial journal. Unpublished directories never gate game startup.
        var j = new Journal();
        string initializing = ".mir2-initializing-" + j.Id;
        Directory.CreateDirectory(PatchEngine.Target(root, initializing));
        Boundary("initial-directory-empty");
        try
        {
            Save(root, j, initializing);
            Directory.Move(PatchEngine.Target(root, initializing), Folder(root));
        }
        catch { RecoverInitializations(root); throw; }
        try
        {
            using var client = source.CreateClient();
            foreach (var info in files)
            {
                cancel.ThrowIfCancellationRequested();
                PatchEngine.ValidateManifestName(info.FileName);
                if (PatchEngine.Protected(info.FileName)) continue;
                string target = PatchEngine.Target(root, info.FileName);
                int index = j.Entries.Count;
                string slot = Slot(root, index);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancel); timeout.CancelAfter(TimeSpan.FromSeconds(60));
                CancellationToken transferToken = timeout.Token;
                using var response = client.GetAsync(PatchEngine.Url(source.Host, info), HttpCompletionOption.ResponseHeadersRead, transferToken).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                long wireLength = info.Compressed == 0 ? info.Length : info.Compressed;
                if (response.Content.Headers.ContentLength is long declared && declared != wireLength) throw new InvalidDataException("HTTP content length mismatch");
                using var raw = response.Content.ReadAsStream(transferToken);
                using var limited = new LimitedStream(raw, wireLength);
                bool compressed = info.Compressed > 0 && info.Compressed != info.Length;
                using var gzip = compressed ? new GZipStream(limited, CompressionMode.Decompress, true) : null;
                Stream input = gzip ?? (Stream)limited;
                using (var output = new FileStream(slot, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[65536]; long total = 0; uint crc = uint.MaxValue; int n;
                    while ((n = input.ReadAsync(buffer, transferToken).AsTask().GetAwaiter().GetResult()) != 0)
                    {
                        cancel.ThrowIfCancellationRequested(); total += n;
                        if (total > info.Length) throw new InvalidDataException("Expanded patch exceeds manifest length");
                        for (int k = 0; k < n; k++) crc = CrcTable[(crc ^ buffer[k]) & 255] ^ (crc >> 8);
                        output.Write(buffer, 0, n); progress?.Invoke(info.FileName, total, info.Length);
                    }
                    if (total != info.Length) throw new InvalidDataException("Downloaded length mismatch");
                    if (limited.Count != wireLength || raw.ReadAsync(new byte[1], transferToken).AsTask().GetAwaiter().GetResult() != 0) throw new InvalidDataException("Wire length mismatch");
                    if (compressed && (limited.Count < 18 || limited.TrailerUInt32(0) != ~crc || limited.TrailerUInt32(4) != (uint)total)) throw new InvalidDataException("Invalid gzip trailer");
                    output.Flush(true);
                }
                File.SetLastWriteTime(slot, info.Creation);
                var e = new Entry { Name = info.FileName, Length = info.Length, Time = info.Creation.ToBinary(), Hash = Hash(slot), Existed = File.Exists(target) };
                if (e.Existed) { e.OldHash = Hash(target); e.OldTime = File.GetLastWriteTime(target).ToBinary(); }
                j.Entries.Add(e); Save(root, j);
            }
            cancel.ThrowIfCancellationRequested(); j.State = "Prepared"; Save(root, j);
        }
        catch { Cleanup(root, j); throw; }
    }
    public static void Apply(string root, Func<bool> confirm) => ApplyConfirmed(root, _ => confirm());
    public static void ApplyConfirmed(string root, Func<string, bool> confirm)
    {
        var j = Load(root);
        if (j.State != "Prepared") throw new IOException("Transaction is not prepared");
        j.State = "Applying"; Save(root, j); Boundary("journal-before-backup");
        try
        {
            for (int i = 0; i < j.Entries.Count; i++)
            {
                var e = j.Entries[i]; InstallationLease.EnsureLegacyClientStopped(root);
                string target = PatchEngine.Target(root, e.Name), stage = Slot(root, i), backup = Slot(root, i, true);
                if (!Matches(stage, e.Hash) || new FileInfo(stage).Length != e.Length) throw new IOException("Staged file integrity failure");
                if (e.Existed)
                {
                    if (!Matches(target, e.OldHash)) throw new IOException("Target changed since staging");
                    CopyDurable(target, backup);
                    if (!Matches(backup, e.OldHash)) throw new IOException("Backup integrity failure");
                }
                else if (File.Exists(target) || Directory.Exists(target)) throw new IOException("New target already exists");
                Boundary("after-backup"); e.Intent = true; Save(root, j); Boundary("journal-before-replace");
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                target = PatchEngine.Target(root, e.Name);
                if (e.Existed) File.Replace(stage, target, null); else File.Move(stage, target);
                Boundary("after-replace");
            }
            j.State = "AwaitingHealth"; j.HealthToken = Guid.NewGuid().ToString("N"); Save(root, j);
            if (!confirm(j.HealthToken)) throw new IOException("Updated launcher startup confirmation failed");
            Boundary("before-commit"); j.State = "Committed"; Save(root, j);
        }
        catch { Rollback(root, j); throw; }
        Cleanup(root, j);
    }
    public static void Recover(string root)
    {
        RecoverCompleted(root);
        if (!Exists(root)) return;
        var j = Load(root);
        if (j.State == "Committed")
        {
            foreach (var e in j.Entries) if (!e.Intent || !Matches(PatchEngine.Target(root, e.Name), e.Hash)) throw new IOException("Committed target missing or changed; retain recovery evidence");
            Cleanup(root, j); return;
        }
        if (j.State is "RolledBack" or "Downloading" or "Prepared") { Cleanup(root, j); return; }
        Rollback(root, j);
    }
    static void Rollback(string root, Journal j)
    {
        j.State = "RollingBack"; Save(root, j);
        for (int i = j.Entries.Count - 1; i >= 0; i--)
        {
            var e = j.Entries[i];
            if (e.Restored)
            {
                string restored = PatchEngine.Target(root, e.Name);
                if (e.Existed ? !Matches(restored, e.OldHash) : File.Exists(restored) || Directory.Exists(restored)) throw new IOException("Restored target changed; evidence retained");
                continue;
            }
            if (!e.Intent) continue;
            string target = PatchEngine.Target(root, e.Name);
            if (e.Existed)
            {
                string backup = Slot(root, i, true);
                if (!Matches(backup, e.OldHash)) throw new IOException("Recovery backup missing or changed; evidence retained");
                if (!Matches(target, e.OldHash))
                {
                    if (!Matches(target, e.Hash)) throw new IOException("Recovery target changed externally; evidence retained");
                    string restore = Internal(root, "restore-" + i);
                    if (!e.RestorePending)
                    {
                        if (File.Exists(restore) || Directory.Exists(restore)) throw new IOException("Unowned recovery scratch; evidence retained");
                        e.RestorePending = true; Save(root, j);
                    }
                    if (File.Exists(restore))
                    {
                        // A killed CopyDurable can leave a prefix, not a complete old file.
                        if (!IsBackupPrefix(restore, backup)) throw new IOException("Recovery scratch changed");
                        File.Delete(Internal(root, "restore-" + i));
                    }
                    CopyDurable(backup, restore); File.SetLastWriteTime(restore, DateTime.FromBinary(e.OldTime));
                    target = PatchEngine.Target(root, e.Name); File.Replace(restore, target, null);
                }
            }
            else if (File.Exists(target))
            {
                if (!Matches(target, e.Hash)) throw new IOException("New target changed externally; refusing deletion");
                File.Delete(PatchEngine.Target(root, e.Name));
            }
            Boundary("during-rollback"); e.Restored = true; Save(root, j);
        }
        j.State = "RolledBack"; Save(root, j); Cleanup(root, j);
    }
    static bool IsBackupPrefix(string scratch, string backup)
    {
        using var partial = File.OpenRead(scratch); using var original = File.OpenRead(backup);
        if (partial.Length > original.Length) return false;
        byte[] a = new byte[65536], b = new byte[65536]; int n;
        while ((n = partial.Read(a)) != 0)
        { original.ReadExactly(b.AsSpan(0, n)); if (!a.AsSpan(0, n).SequenceEqual(b.AsSpan(0, n))) return false; }
        return true;
    }
    static void RecoverInitializations(string root)
    {
        foreach (string folder in Directory.GetDirectories(root, ".mir2-initializing-*", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileName(folder), id = name[".mir2-initializing-".Length..];
            // No marker exists before the first durable journal. Preserve such or unknown
            // evidence, but do not confuse it with a published transaction or block retries.
            try
            {
                if (!Guid.TryParseExact(id, "N", out _)) continue;
                PatchEngine.Target(root, name);
                var j = Load(root, name);
                if (j.Id != id || j.State != "Downloading" || j.Entries.Count != 0 || j.HealthToken != null) continue;
                var entries = Directory.GetFileSystemEntries(folder);
                if (entries.Length != 1 || Path.GetFileName(entries[0]) != "journal.json") continue;
                File.Delete(PatchEngine.Target(root, name + "/journal.json"));
                Directory.Delete(PatchEngine.Target(root, name), false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or CryptographicException or System.ComponentModel.Win32Exception or ArgumentException)
            { /* Unpublished evidence is never recursively erased or allowed to gate startup. */ }
        }
    }
    static void RecoverCompleted(string root)
    {
        RecoverInitializations(root);
        foreach (string folder in Directory.GetDirectories(root, ".mir2-completed-*", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileName(folder), id = name[".mir2-completed-".Length..];
            if (!Guid.TryParseExact(id, "N", out _)) throw new IOException("Unknown completed transaction artifact");
            PatchEngine.Target(root, name);
            if (!File.Exists(PatchEngine.Target(root, name + "/journal.json")))
            {
                // A kill after the final journal deletion can leave only an empty directory.
                Directory.Delete(folder, false); continue;
            }
            var j = Load(root, name);
            if (j.Id != id || j.State is "Applying" or "AwaitingHealth" or "RollingBack") throw new IOException("Invalid completed transaction state");
            Cleanup(root, j, name);
        }
    }
    static void Cleanup(string root, Journal j, string folderName = DirectoryName)
    {
        // Never recursive-delete: unknown entries survive and block subsequent updates.
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "journal.json", "journal.next", "RECOVERY.txt" };
        if (j.HealthToken != null) allowed.Add("health-" + j.HealthToken);
        for (int i = 0; i <= j.Entries.Count; i++) foreach (string prefix in new[] { "stage-", "backup-", "restore-" }) allowed.Add(prefix + i);
        foreach (string path in Directory.GetFileSystemEntries(PatchEngine.Target(root, folderName)))
        {
            if (!allowed.Contains(Path.GetFileName(path)) || (File.GetAttributes(path) & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0) throw new IOException("Unknown transaction artifact; evidence retained");
        }
        if (folderName == DirectoryName)
        {
            string completed = ".mir2-completed-" + j.Id;
            Directory.Move(Folder(root), PatchEngine.Target(root, completed)); folderName = completed;
            Boundary("cleanup-after-rename");
        }
        string Artifact(string name) => PatchEngine.Target(root, folderName + "/" + name);
        for (int i = 0; i <= j.Entries.Count; i++)
            foreach (string prefix in new[] { "stage-", "backup-", "restore-" })
            { string path = Artifact(prefix + i); if (File.Exists(path)) File.Delete(path); }
        if (j.HealthToken != null)
        { string ack = Artifact("health-" + j.HealthToken); if (File.Exists(ack)) File.Delete(ack); }
        foreach (string name in new[] { "RECOVERY.txt", "journal.next", "journal.json" })
        { string path = Artifact(name); if (File.Exists(path)) File.Delete(path); }
        Directory.Delete(PatchEngine.Target(root, folderName), false);
    }
    static readonly uint[] CrcTable = Enumerable.Range(0, 256).Select(v => { uint c = (uint)v; for (int i = 0; i < 8; i++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1; return c; }).ToArray();
    sealed class LimitedStream(Stream source, long limit) : Stream
    {
        public long Count { get; private set; }
        readonly byte[] trailer = new byte[8];
        public uint TrailerUInt32(int offset)
        {
            uint value = 0; for (int i = 0; i < 4; i++) value |= (uint)trailer[(int)((Count + offset + i) % 8)] << (8 * i); return value;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Count == limit) return 0;
            int n = source.Read(buffer, offset, (int)Math.Min(count, limit - Count));
            for (int i = 0; i < n; i++) trailer[(int)((Count + i) % 8)] = buffer[offset + i];
            Count += n; return n;
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            if (Count == limit) return 0;
            int n = await source.ReadAsync(buffer[..(int)Math.Min(buffer.Length, limit - Count)], token).ConfigureAwait(false);
            for (int i = 0; i < n; i++) trailer[(int)((Count + i) % 8)] = buffer.Span[i];
            Count += n; return n;
        }
        public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false;
        public override long Length => limit; public override long Position { get => Count; set => throw new NotSupportedException(); }
        public override void Flush() { } public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
        public override void SetLength(long v) => throw new NotSupportedException(); public override void Write(byte[] b, int o, int c) => throw new NotSupportedException();
    }
}
