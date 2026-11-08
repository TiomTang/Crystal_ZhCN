// Offline localization maintenance. This tool is not part of the build and never searches user
// directories. Published client language files are read-only at runtime; maintain their source JSON here.
//
// Run from the repository root (the two optional positional arguments remain root and output):
//   dotnet run --project tools/LocalizationScan
//   dotnet run --project tools/LocalizationScan -- . C:/work/LocalizationCatalog.g.cs
//   dotnet run --project tools/LocalizationScan -- --language Client/Localization/Chinese.json --scope client
//   dotnet run --project tools/LocalizationScan -- --language Client/Localization/Chinese.json --scope client --capture C:/work/captures --capture C:/work/session.json
//   dotnet run --project tools/LocalizationScan -- --language Client/Localization/Chinese.json --scope client --check
//
// --language requires an existing valid UTF-8 JSON object (an empty {} is allowed). New entries have
// Source set to the source template and Text empty. Existing entries, translations and metadata are
// never replaced or removed. Captures add pending keys only: their Text and __meta are not imported.
// --scope limits static language requirements, never the global generated catalog. Explicit captures
// have no inferred client/server ownership; supply only the captures wanted in this language file.
// --check writes nothing, fails for a stale/missing catalog, missing required keys or mismatched
// numeric format items in existing translations, and reports untranslated entries without failing.
//
// Static extraction handles direct literals and complete interpolated templates, not variable/enum
// values, dynamic UI or arbitrary constant expressions. Skipped calls need explicit runtime captures.
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

return LocalizationTool.Run(args);

internal static class LocalizationTool
{
    internal static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static int Run(string[] arguments)
    {
        try
        {
            if (arguments.Contains("--help") || arguments.Contains("-h"))
            {
                PrintHelp();
                return 0;
            }

            var options = Options.Parse(arguments);
            if (!Directory.Exists(options.Root)) throw new IOException($"Repository root does not exist: {options.Root}");
            if (options.Language != null && PathComparer.Equals(options.Output, options.Language))
                throw new ArgumentException("Catalog output and language input must be different files.");

            // Read and validate every input before preparing either output. Bad JSON must not cause
            // an empty-table rebuild, even when the generated catalog is also out of date.
            var language = options.Language == null ? null : LanguageFile.Read(options.Language);
            var captures = ReadCaptures(options);
            var scan = SourceScanner.Scan(options.Root, options.Output, options.Scope);
            var catalog = BuildCatalog(scan.All);
            var oldCatalog = TextSnapshot.ReadOptional(options.Output);
            var catalogCurrent = oldCatalog != null && NormalizeLines(oldCatalog.Text) == NormalizeLines(catalog);
            Console.WriteLine($"scanned {scan.Files} source files: {scan.All.Count} global templates, {scan.Selected.Count} in {options.Scope.ToString().ToLowerInvariant()} scope; skipped {scan.Skipped} non-static Get expressions");

            var required = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var template in scan.Selected) required.Add(template, template);
            foreach (var capture in captures)
            {
                foreach (var entry in capture.Entries)
                {
                    // A capture is evidence of a missing/untranslated key, never a translation source.
                    if (language!.Entries.ContainsKey(entry.Key)) continue;
                    if (required.TryGetValue(entry.Key, out var source) && source != entry.Value.Source)
                        throw new InvalidDataException($"Conflicting captured source for {Display(entry.Key)} in {capture.Snapshot.Path}.");
                    required[entry.Key] = entry.Value.Source;
                }
            }

            var missing = language == null ? new List<KeyValuePair<string, string>>()
                : required.Where(entry => !language.Entries.ContainsKey(entry.Key)).ToList();
            if (options.Check)
            {
                var failures = 0;
                if (!catalogCurrent)
                {
                    Console.Error.WriteLine($"Catalog is missing or stale: {options.Output}");
                    failures++;
                }
                if (language != null)
                {
                    foreach (var entry in missing) Console.Error.WriteLine($"Missing key: {Display(entry.Key)}");
                    var invalid = CheckTranslations(language);
                    var untranslated = language.Entries.Count(entry => string.IsNullOrEmpty(entry.Value.Text));
                    Console.WriteLine($"language {language.Snapshot.Path}: required={required.Count}, missing={missing.Count}, untranslated={untranslated}, placeholder-errors={invalid}");
                    failures += missing.Count + invalid;
                }
                Console.WriteLine(failures == 0 ? "check passed (no files written)" : "check failed (no files written)");
                return failures == 0 ? 0 : 1;
            }

            var writes = new List<PendingWrite>();
            if (!catalogCurrent)
            {
                var bytes = oldCatalog == null ? new UTF8Encoding(false).GetBytes(catalog)
                    : oldCatalog.Encode(catalog.Replace("\r\n", "\n").Replace("\n", oldCatalog.NewLine));
                writes.Add(new PendingWrite(options.Output, bytes, oldCatalog));
            }
            if (language != null && missing.Count != 0)
                writes.Add(new PendingWrite(language.Snapshot.Path, language.Append(missing), language.Snapshot));

            // Detect concurrent edits before writing, rather than replacing a translator's newer file.
            foreach (var write in writes) write.VerifyUnchanged();
            foreach (var write in writes) write.Save();
            Console.WriteLine($"catalog {(catalogCurrent ? "unchanged" : "updated")} -> {options.Output}");
            if (language != null)
                Console.WriteLine($"language {(missing.Count == 0 ? "unchanged" : "updated")}: added={missing.Count}, untranslated={language.Entries.Count(entry => string.IsNullOrEmpty(entry.Value.Text)) + missing.Count} -> {language.Snapshot.Path}");
            return 0;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or System.Text.Json.JsonException or DecoderFallbackException or NotSupportedException)
        {
            Console.Error.WriteLine($"LocalizationScan: {exception.Message}");
            return 2;
        }
    }

    private static List<LanguageFile> ReadCaptures(Options options)
    {
        var paths = new SortedSet<string>(PathComparer);
        foreach (var input in options.Captures)
        {
            if ((File.GetAttributes(input) & FileAttributes.Directory) == 0)
            {
                paths.Add(input);
                continue;
            }

            var files = Directory.EnumerateFiles(input, "*.json", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.ReparsePoint,
                MatchCasing = MatchCasing.CaseInsensitive
            }).Select(Path.GetFullPath).ToList();
            if (files.Count == 0) throw new InvalidDataException($"Capture directory contains no JSON files: {input}");
            paths.UnionWith(files);
        }

        var result = new List<LanguageFile>();
        foreach (var path in paths)
        {
            if (PathComparer.Equals(path, options.Output))
                throw new ArgumentException("Catalog output must not overwrite a capture input.");
            result.Add(LanguageFile.Read(path));
        }
        return result;
    }

    private static int CheckTranslations(LanguageFile language)
    {
        var invalid = 0;
        // Include stable enum/UI keys and retained unused entries: static scanning cannot determine
        // their ownership, but every nonempty translation in the selected language must be safe.
        foreach (var entry in language.Entries)
        {
            if (string.IsNullOrEmpty(entry.Value.Text)) continue;
            var problem = FormatItems.Compare(entry.Value.Source, entry.Value.Text);
            if (problem == null) continue;
            Console.Error.WriteLine($"Invalid placeholders for {Display(entry.Key)}: {problem}");
            invalid++;
        }
        return invalid;
    }

    private static string BuildCatalog(IEnumerable<string> templates)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("// Generated by Tools/LocalizationScan. Run it after adding localization calls.");
        builder.AppendLine("public static class LocalizationCatalog");
        builder.AppendLine("{");
        builder.AppendLine("    public static readonly string[] Templates =");
        builder.AppendLine("    {");
        foreach (var template in templates)
            builder.Append("        ").Append(SymbolDisplay.FormatLiteral(template, true)).AppendLine(",");
        builder.AppendLine("    };");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string NormalizeLines(string text) => text.Replace("\r\n", "\n");
    private static string Display(string text) => System.Text.Json.JsonSerializer.Serialize(text);

    private static void PrintHelp()
    {
        Console.WriteLine("""
            Usage: LocalizationScan [root] [output] [--language <json>] [--capture <file-or-directory>]... [--scope client|server|all] [--check]

            Defaults: discover the repository root; output is <root>/Shared/LocalizationCatalog.g.cs.
            --language  Merge into an existing valid UTF-8 language JSON object; never remove keys or overwrite existing entries/metadata.
            --capture   Import pending keys from an explicit JSON file or directory (recursive, repeatable); requires --language. Capture Text and metadata are ignored.
            --scope     Static language requirements: client = Client + Launcher + Shared; server = Server + Server.MirForms + Shared; all = all production source (default).
                        The catalog always contains every production template. Captures are explicit, not filtered or discovered by scope.
            --check     No writes, directories or temporary files. Check catalog freshness, required keys, and numeric format items in every nonempty translation of --language.
                        Format items may be reordered, but indices, repeated occurrences, alignment and format specifiers must match Source. Untranslated entries are reported, not errors.
            --help, -h  Show this help. Exit codes: 0 success, 1 check failed, 2 invalid input or I/O failure.

            Run from the repository root:
              dotnet run --project tools/LocalizationScan
              dotnet run --project tools/LocalizationScan -- . C:/work/LocalizationCatalog.g.cs
              dotnet run --project tools/LocalizationScan -- --language Client/Localization/Chinese.json --scope client
              dotnet run --project tools/LocalizationScan -- --language Client/Localization/Chinese.json --scope client --capture C:/work/captures --capture C:/work/session.json
              dotnet run --project tools/LocalizationScan -- --language Server.MirForms/Localization/Chinese.json --scope server --check

            Only complete direct string/interpolated Get templates are extracted. Variables, enums and dynamic UI need runtime captures; unsupported expressions are skipped, not guessed.
            """);
    }

    private sealed record Options(string Root, string Output, string? Language, List<string> Captures, ScanScope Scope, bool Check)
    {
        public static Options Parse(string[] arguments)
        {
            var positional = new List<string>();
            var captures = new List<string>();
            string? language = null;
            var scope = ScanScope.All;
            var seenScope = false;
            var check = false;
            var positionalOnly = false;
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                if (!positionalOnly && argument == "--") { positionalOnly = true; continue; }
                if (positionalOnly || !argument.StartsWith('-')) { positional.Add(argument); continue; }
                switch (argument)
                {
                    case "--language":
                        if (language != null) throw new ArgumentException("--language may only be supplied once.");
                        language = Path.GetFullPath(Value());
                        break;
                    case "--capture": captures.Add(Path.GetFullPath(Value())); break;
                    case "--scope":
                        if (seenScope) throw new ArgumentException("--scope may only be supplied once.");
                        scope = Value() switch
                        {
                            "client" => ScanScope.Client,
                            "server" => ScanScope.Server,
                            "all" => ScanScope.All,
                            _ => throw new ArgumentException("--scope must be client, server or all.")
                        };
                        seenScope = true;
                        break;
                    case "--check": check = true; break;
                    default: throw new ArgumentException($"Unknown option: {argument}. Use --help.");
                }

                string Value()
                {
                    if (++i >= arguments.Length || string.IsNullOrWhiteSpace(arguments[i]) || arguments[i].StartsWith("--"))
                        throw new ArgumentException($"Missing value for {argument}.");
                    return arguments[i];
                }
            }
            if (positional.Count > 2) throw new ArgumentException("Expected at most two positional arguments: [root] [output].");
            if (captures.Count != 0 && language == null) throw new ArgumentException("--capture requires --language.");
            var root = positional.Count > 0 ? Path.GetFullPath(positional[0]) : FindRoot();
            var output = positional.Count > 1 ? Path.GetFullPath(positional[1]) : Path.Combine(root, "Shared", "LocalizationCatalog.g.cs");
            return new Options(root, output, language, captures, scope, check);
        }

        private static string FindRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var git = Path.Combine(directory.FullName, ".git");
                if (Directory.Exists(git) || File.Exists(git)) return directory.FullName;
                directory = directory.Parent;
            }
            return Directory.GetCurrentDirectory();
        }
    }
}
