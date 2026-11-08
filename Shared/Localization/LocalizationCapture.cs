#if DEBUG
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

/// <summary>
/// Opt-in developer diagnostics, separate from the published translation table. Each process
/// and language load owns a unique file, so multiple games/launchers never overwrite one another.
/// Files contain templates, not formatted argument values, and are never loaded as translations.
/// </summary>
internal sealed class LocalizationCapture : IDisposable
{
    private readonly object _sync = new object();
    private readonly object _writeSync = new object();
    private readonly Dictionary<string, LocalizationEntry> _entries = new Dictionary<string, LocalizationEntry>(StringComparer.Ordinal);
    private readonly string _path;
    private Timer _timer;
    private bool _dirty;
    private bool _disposed;
    private bool _failed;
    private long _version;

    private LocalizationCapture(string path)
    {
        _path = path;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Trace.WriteLine("[LocalizationCapture] Developer collection enabled: " + path);
    }

    internal static LocalizationCapture Create(string languagePath)
    {
        if (Environment.GetEnvironmentVariable("MIR2_LOCALIZATION_COLLECT") != "1") return null;
        try
        {
            string languageFile = Path.GetFullPath(languagePath);
            string process = SafeName(Assembly.GetEntryAssembly()?.GetName().Name ?? "Client");
            string language = SafeName(Path.GetFileNameWithoutExtension(languageFile));
            string filename = language + ".Debug." + process + "."
                + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff", CultureInfo.InvariantCulture)
                + "-" + Environment.ProcessId + "-" + Guid.NewGuid().ToString("N") + ".json";
            string path = Path.Combine(Path.GetDirectoryName(languageFile), filename);
            return new LocalizationCapture(path);
        }
        catch (Exception ex)
        {
            Trace.WriteLine("[LocalizationCapture] Collection disabled: " + ex);
            return null;
        }
    }

    private static string SafeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        string safe = new string((name ?? string.Empty).Select(c => invalid.Contains(c) ? '_' : c).ToArray()).TrimEnd('.', ' ');
        return string.IsNullOrEmpty(safe) ? "Unknown" : safe;
    }

    internal void Record(string key, string source)
    {
        if (string.IsNullOrEmpty(key)) return;
        lock (_sync)
        {
            if (_disposed || _failed || _entries.ContainsKey(key)) return;
            try
            {
                _entries.Add(key, new LocalizationEntry { Source = source ?? string.Empty, Text = string.Empty });
                _dirty = true;
                _version++;
                _timer ??= new Timer(_ => Flush(), null, Timeout.Infinite, Timeout.Infinite);
                _timer.Change(2000, Timeout.Infinite);
            }
            catch (Exception ex) { Fail(ex); }
        }
    }

    private void Flush()
    {
        // Only writers wait for disk IO; Record never takes this gate.
        lock (_writeSync)
        {
            KeyValuePair<string, LocalizationEntry>[] snapshot;
            long version;
            lock (_sync)
            {
                if (_failed || !_dirty) return;
                snapshot = _entries.ToArray();
                version = _version;
            }
            try
            {
                using var stream = new MemoryStream();
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                {
                    Indented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("__meta");
                    writer.WriteStartObject();
                    writer.WriteNumber("version", 2);
                    writer.WritePropertyName("forms");
                    writer.WriteStartArray(); writer.WriteEndArray();
                    writer.WriteEndObject();
                    foreach (var item in snapshot)
                    {
                        writer.WritePropertyName(item.Key);
                        writer.WriteStartObject();
                        writer.WriteString("Source", item.Value.Source);
                        writer.WriteString("Text", string.Empty);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                }
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
                string temporary = _path + ".tmp";
                File.WriteAllBytes(temporary, stream.ToArray());
                File.Move(temporary, _path, true);
                lock (_sync) _dirty = _version != version;
            }
            catch (Exception ex) { Fail(ex); }
        }
    }

    private void Fail(Exception exception)
    {
        lock (_sync)
        {
            if (_failed) return;
            _failed = true;
            _timer?.Dispose();
        }
        Trace.WriteLine("[LocalizationCapture] Collection stopped: " + _path + Environment.NewLine + exception);
    }

    private void OnProcessExit(object sender, EventArgs args) => Dispose();

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            _timer?.Dispose();
        }
        Flush();
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
    }
}
#endif
