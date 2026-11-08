using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

internal sealed record TextSnapshot(string Path, byte[] Bytes, string Text, bool HasBom, string NewLine)
{
    public static TextSnapshot Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var bom = bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF });
        var text = new UTF8Encoding(false, true).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
        var newline = Environment.NewLine;
        var first = text.IndexOfAny(new[] { '\r', '\n' });
        if (first >= 0) newline = text[first] == '\r' ? (first + 1 < text.Length && text[first + 1] == '\n' ? "\r\n" : "\r") : "\n";
        return new TextSnapshot(path, bytes, text, bom, newline);
    }

    public static TextSnapshot? ReadOptional(string path)
    {
        try { return Read(path); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    public byte[] Encode(string text)
    {
        var bytes = new UTF8Encoding(false).GetBytes(text);
        return HasBom ? new byte[] { 0xEF, 0xBB, 0xBF }.Concat(bytes).ToArray() : bytes;
    }
}

internal sealed record PendingWrite(string Path, byte[] Bytes, TextSnapshot? Original)
{
    public void VerifyUnchanged()
    {
        var current = TextSnapshot.ReadOptional(Path);
        if (Original == null ? current != null : current == null || !Original.Bytes.AsSpan().SequenceEqual(current.Bytes))
            throw new IOException($"File changed during scanning; refusing to overwrite: {Path}");
    }

    public void Save()
    {
        VerifyUnchanged();
        var directory = System.IO.Path.GetDirectoryName(Path)!;
        Directory.CreateDirectory(directory);
        var temporary = System.IO.Path.Combine(directory, System.IO.Path.GetFileName(Path) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllBytes(temporary, Bytes);
            File.Move(temporary, Path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}

internal sealed record LanguageEntry(string Source, string Text);

internal sealed record LanguageFile(TextSnapshot Snapshot, Dictionary<string, LanguageEntry> Entries, bool HasProperties)
{
    public static LanguageFile Read(string path)
    {
        var snapshot = TextSnapshot.Read(path);
        try
        {
            using var document = JsonDocument.Parse(snapshot.Text);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Expected a language JSON object.");
            ValidateUniqueProperties(root);
            var entries = new Dictionary<string, LanguageEntry>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
            {
                var value = property.Value;
                if (property.Name == "__meta")
                {
                    if (value.ValueKind != JsonValueKind.Object) throw new InvalidDataException("__meta must be an object.");
                    if (value.TryGetProperty("forms", out var forms)
                        && (forms.ValueKind != JsonValueKind.Array || forms.EnumerateArray().Any(form => form.ValueKind != JsonValueKind.String)))
                        throw new InvalidDataException("__meta.forms must be an array of strings.");
                    continue;
                }
                if (property.Name.Length == 0 || value.ValueKind != JsonValueKind.Object
                    || !value.TryGetProperty("Source", out var source) || source.ValueKind != JsonValueKind.String
                    || !value.TryGetProperty("Text", out var text) || text.ValueKind != JsonValueKind.String)
                    throw new InvalidDataException($"Entry {JsonSerializer.Serialize(property.Name)} must contain string Source and Text fields.");
                entries.Add(property.Name, new LanguageEntry(source.GetString()!, text.GetString()!));
            }
            return new LanguageFile(snapshot, entries, root.EnumerateObject().Any());
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        {
            throw new InvalidDataException($"Invalid language/capture JSON {path}: {exception.Message}", exception);
        }
    }

    public byte[] Append(IReadOnlyList<KeyValuePair<string, string>> additions)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            writer.WriteStartObject();
            foreach (var entry in additions)
            {
                writer.WritePropertyName(entry.Key);
                writer.WriteStartObject();
                writer.WriteString("Source", entry.Value);
                writer.WriteString("Text", string.Empty);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        var serialized = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n");
        var body = serialized[(serialized.IndexOf('\n') + 1)..serialized.LastIndexOf('\n')];
        body = body.Replace("\n", Snapshot.NewLine);
        var closing = Snapshot.Text.LastIndexOf('}');
        var insertion = closing;
        while (insertion > 0 && char.IsWhiteSpace(Snapshot.Text[insertion - 1])) insertion--;
        var suffix = Snapshot.Text[insertion..];
        if (!suffix.StartsWith('\r') && !suffix.StartsWith('\n')) suffix = Snapshot.NewLine + suffix;
        // Append rather than reserialize: existing order, custom fields, metadata, translations,
        // escaping, trailing whitespace, BOM and line endings all remain byte-for-byte intact.
        var text = Snapshot.Text[..insertion] + (HasProperties ? "," : string.Empty) + Snapshot.NewLine + body + suffix;
        return Snapshot.Encode(text);
    }

    private static void ValidateUniqueProperties(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray()) ValidateUniqueProperties(item);
            return;
        }
        if (value.ValueKind != JsonValueKind.Object) return;
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!names.Add(property.Name)) throw new InvalidDataException($"Duplicate property: {JsonSerializer.Serialize(property.Name)}.");
            ValidateUniqueProperties(property.Value);
        }
    }
}

internal static class FormatItems
{
    private sealed record Item(int Index, int Alignment, string Format);

    public static string? Compare(string source, string translation)
    {
        // Braces in plain UI text are not necessarily composite formatting. Only enforce composite
        // syntax when either side contains an unescaped numeric format-item start.
        if (!HasNumericItem(source) && !HasNumericItem(translation)) return null;
        Dictionary<Item, int> expected;
        Dictionary<Item, int> actual;
        try { expected = Parse(source); }
        catch (Exception exception) when (exception is FormatException or OverflowException) { return "Source contains malformed numeric composite formatting."; }
        try { actual = Parse(translation); }
        catch (Exception exception) when (exception is FormatException or OverflowException) { return "Text contains malformed numeric composite formatting."; }
        return expected.Count == actual.Count && expected.All(pair => actual.TryGetValue(pair.Key, out var count) && count == pair.Value)
            ? null : "Numeric indices, occurrence counts, alignment or format specifiers differ from Source.";
    }

    private static bool HasNumericItem(string text)
    {
        for (var i = 0; i + 1 < text.Length; i++)
        {
            if (text[i] == '{' && text[i + 1] == '{') { i++; continue; }
            if (text[i] == '{' && char.IsAsciiDigit(text[i + 1])) return true;
        }
        return false;
    }

    private static Dictionary<Item, int> Parse(string text)
    {
        // Use the target runtime's parser as the syntax authority, not a regex approximation.
        _ = CompositeFormat.Parse(text);
        var items = new Dictionary<Item, int>();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '{') continue;
            if (i + 1 < text.Length && text[i + 1] == '{') { i++; continue; }
            var start = ++i;
            while (char.IsAsciiDigit(text[i])) i++;
            var index = int.Parse(text[start..i], CultureInfo.InvariantCulture);
            while (text[i] == ' ') i++;
            var alignment = 0;
            if (text[i] == ',')
            {
                i++;
                while (text[i] == ' ') i++;
                start = i;
                if (text[i] == '-') i++;
                while (char.IsAsciiDigit(text[i])) i++;
                alignment = int.Parse(text[start..i], CultureInfo.InvariantCulture);
                while (text[i] == ' ') i++;
            }
            var format = string.Empty;
            if (text[i] == ':')
            {
                start = ++i;
                while (text[i] != '}') i++;
                format = text[start..i];
            }
            var item = new Item(index, alignment, format);
            items[item] = items.TryGetValue(item, out var count) ? count + 1 : 1;
        }
        return items;
    }
}
