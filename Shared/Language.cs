using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Runtime localization for game and server UI text. Client language packs are published,
/// read-only resources; server-side discovery retains its existing writable policy.
/// </summary>
public static class GameLanguage
{
    public static readonly LocalizationStore ServerStore = new LocalizationStore();
    public static readonly LocalizationStore ClientStore = new LocalizationStore(readOnly: true);

#if DEBUG
    private static LocalizationCapture _clientCapture;
#endif

    private static LocalizationStore Store
    {
        get { return Packet.IsServer ? ServerStore : ClientStore; }
    }

    /// <summary>
    /// Language whose keys are already English, so it needs no file: missing translations fall back
    /// to the key itself and runtime additions are never written back.
    /// </summary>
    public const string SourceLanguage = "English";

    /// <summary>
    /// Loads a published pack without seeding, modifying or saving it. Missing text falls back
    /// to English. In Debug only, MIR2_LOCALIZATION_COLLECT=1 records missing/untranslated keys
    /// beside the pack as {language}.Debug.{process}.{unique-session}.json.
    /// Capture files are developer input for tools/LocalizationScan, not runtime overrides.
    /// </summary>
    public static void LoadClientLanguage(string languageJsonPath)
    {
#if DEBUG
        LocalizationCapture previous;
#endif
        // Publish the pack and its collector together, including concurrent reloads/lookups.
        lock (ClientStore.SyncRoot)
        {
#if DEBUG
            ClientStore.CaptureMissing = null;
            previous = _clientCapture;
#endif
            ClientStore.Load(languageJsonPath, saveChanges: false);
#if DEBUG
            _clientCapture = LocalizationCapture.Create(languageJsonPath);
            if (_clientCapture != null) ClientStore.CaptureMissing = _clientCapture.Record;
#endif
        }
#if DEBUG
        // Retiring a collector may flush to disk; never hold the translation lock for that IO.
        previous?.Dispose();
#endif
    }

    public static void LoadServerLanguage(string languageJsonPath)
    {
        ServerStore.Load(languageJsonPath, !IsSourceLanguage(languageJsonPath));
        Seed(ServerStore);
    }

    private static bool IsSourceLanguage(string path) =>
        string.Equals(Path.GetFileNameWithoutExtension(path), SourceLanguage, StringComparison.OrdinalIgnoreCase);

    public static void SaveServerLanguage(string languageJsonPath) => ServerStore.Save(languageJsonPath);

    /// <summary>
    /// Registers every template found by the build-time scanner, so a fresh install starts with a
    /// complete table instead of waiting for each line of text to be executed.
    /// </summary>
    private static void Seed(LocalizationStore store)
    {
        foreach (var template in LocalizationCatalog.Templates)
        {
            store.TryAdd(template, template);
        }
    }

    /// <summary>
    /// Translates an interpolated string. The compiler supplies the format string, so argument
    /// values never become part of the key.
    /// </summary>
    public static string Get(LocalizationStringHandler text)
    {
        return Resolve(text.Template, text.Arguments, compositeFormat: true);
    }

    /// <summary>
    /// Translates a plain string, used when there is nothing to interpolate.
    /// </summary>
    public static string Get(string template)
    {
        return Resolve(template, Array.Empty<object>());
    }

    /// <summary>
    /// Translates an explicit template with {0} placeholders. Used for dynamic templates.
    /// </summary>
    public static string Get(string template, params object[] args)
    {
        return Resolve(template, args);
    }

    private static string Resolve(string template, object[] args, bool compositeFormat = false)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        var translation = Store.Translate(template, template);
        var formatted = Format(translation, args, compositeFormat);

        return formatted ?? Format(template, args, compositeFormat) ?? template;
    }

    private static string Format(string text, object[] args, bool compositeFormat)
    {
        if (string.IsNullOrEmpty(text) || args == null || (!compositeFormat && args.Length == 0)) return text;

        try
        {
            return string.Format(text, args);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Translates an enum value through the "enum.Type_Value" key, falling back to the enum name.
    /// </summary>
    public static string Get(Enum value)
    {
        var name = value.ToString();
        return Store.Translate("enum." + value.GetType().Name + "_" + name, name);
    }

    /// <summary>
    /// Captures the format string and arguments of an interpolated string without formatting it,
    /// so the template can be used as the localization key.
    /// </summary>
    [InterpolatedStringHandler]
    public ref struct LocalizationStringHandler
    {
        private readonly StringBuilder _template;
        private readonly object[] _arguments;
        private int _index;

        public LocalizationStringHandler(int literalLength, int formattedCount)
        {
            _template = new StringBuilder(literalLength + formattedCount * 4);
            _arguments = formattedCount == 0 ? Array.Empty<object>() : new object[formattedCount];
        }

        public void AppendLiteral(string value) => _template.Append(value.Replace("{", "{{").Replace("}", "}}"));

        public void AppendFormatted<T>(T value)
        {
            _arguments[_index] = value;
            _template.Append('{').Append(_index).Append('}');
            _index++;
        }

        public void AppendFormatted<T>(T value, string format)
        {
            _arguments[_index] = value;
            _template.Append('{').Append(_index).Append(':').Append(format).Append('}');
            _index++;
        }

        public void AppendFormatted<T>(T value, int alignment)
        {
            _arguments[_index] = value;
            _template.Append('{').Append(_index).Append(',').Append(alignment).Append('}');
            _index++;
        }

        public void AppendFormatted<T>(T value, int alignment, string format)
        {
            _arguments[_index] = value;
            _template.Append('{').Append(_index).Append(',').Append(alignment).Append(':').Append(format).Append('}');
            _index++;
        }

        public string Template => _template.ToString();
        public object[] Arguments => _arguments;
    }
}
