using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal enum ScanScope { All, Client, Server }

internal sealed record ScanResult(SortedSet<string> All, SortedSet<string> Selected, int Files, int Skipped);

internal static class SourceScanner
{
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Build", "bin", "obj", "tools", "test", "tests", "testing", "docs", "doc", "documentation",
        "cache", "caches", "packages", "node_modules", "artifacts", "dist", "coverage", "TestResults",
        "tmp", "temp", "backup", "backups", "Resources", "Assets"
    };

    public static ScanResult Scan(string root, string output, ScanScope scope)
    {
        var all = new SortedSet<string>(StringComparer.Ordinal);
        var selected = new SortedSet<string>(StringComparer.Ordinal);
        var sources = new List<(SyntaxTree Tree, bool InScope, bool HasCalls)>();
        var files = 0;
        var skipped = 0;
        foreach (var file in EnumerateSources(root))
        {
            var name = Path.GetFileName(file);
            if (name.Equals("LocalizationCatalog.g.cs", StringComparison.OrdinalIgnoreCase)
                || LocalizationTool.PathComparer.Equals(Path.GetFullPath(file), output)) continue;
            var implementation = name.Equals("Language.cs", StringComparison.OrdinalIgnoreCase)
                || name.Equals("LocalizationStore.cs", StringComparison.OrdinalIgnoreCase);
            files++;
            // An unreadable formal source is an error, not proof that the catalog is complete.
            var text = File.ReadAllText(file);
            var relative = Path.GetRelativePath(root, file);
            var component = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
            var inScope = scope == ScanScope.All || component.Equals("Shared", StringComparison.OrdinalIgnoreCase)
                || (scope == ScanScope.Client && (component.Equals("Client", StringComparison.OrdinalIgnoreCase) || component.Equals("Launcher", StringComparison.OrdinalIgnoreCase)))
                || (scope == ScanScope.Server && (component.Equals("Server", StringComparison.OrdinalIgnoreCase) || component.Equals("Server.MirForms", StringComparison.OrdinalIgnoreCase)));
            sources.Add((CSharpSyntaxTree.ParseText(text, path: file), inScope, !implementation && text.Contains("GameLanguage", StringComparison.Ordinal)));
        }

        // Roslyn constant evaluation avoids guessing alignment expressions, and distinguishes
        // constant interpolated strings (which select Get(string)) from the handler overload.
        // No game references or build are needed; unresolved constants are not evaluated.
        var compilation = CSharpCompilation.Create("LocalizationScan.Constants", sources.Select(source => source.Tree),
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        foreach (var source in sources.Where(source => source.HasCalls))
        {
            var model = compilation.GetSemanticModel(source.Tree);
            foreach (var invocation in source.Tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax member || member.Name.Identifier.ValueText != "Get") continue;
                if (member.Expression is not IdentifierNameSyntax receiver || receiver.Identifier.ValueText != "GameLanguage") continue;
                var arguments = invocation.ArgumentList.Arguments;
                var expression = arguments.Count == 0 ? null : arguments[0].Expression;
                var template = expression == null ? null : FromExpression(expression, arguments.Count == 1, model);
                if (template == null) { skipped++; continue; }
                if (template.Length == 0) continue;
                all.Add(template);
                if (source.InScope) selected.Add(template);
            }
        }
        return new ScanResult(all, selected, files, skipped);
    }

    private static IEnumerable<string> EnumerateSources(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.cs")) yield return file;
        foreach (var child in Directory.EnumerateDirectories(directory))
        {
            var name = Path.GetFileName(child);
            if (name.StartsWith('.') || ExcludedDirectories.Contains(name)
                || name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".Test", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Tests.", StringComparison.OrdinalIgnoreCase)
                || (File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0) continue;
            foreach (var file in EnumerateSources(child)) yield return file;
        }
    }

    private static string? FromExpression(ExpressionSyntax expression, bool handlerAllowed, SemanticModel model)
    {
        while (expression is ParenthesizedExpressionSyntax parentheses) expression = parentheses.Expression;
        if (expression.ContainsDiagnostics) return null;
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            return literal.Token.ValueText;
        // Interpolation passed to the string/params overload is already evaluated at runtime; its
        // template cannot be recovered safely. Likewise do not invent keys for casts/concatenations.
        if (!handlerAllowed || expression is not InterpolatedStringExpressionSyntax interpolated) return null;
        var constant = model.GetConstantValue(interpolated);
        if (constant.HasValue && constant.Value is string constantText) return constantText;
        // An unresolved expression might be an external string constant, which would select the
        // string overload instead. Do not claim a handler key when that cannot be established.
        foreach (var item in interpolated.Contents.OfType<InterpolationSyntax>())
        {
            var type = model.GetTypeInfo(item.Expression).Type;
            if (type == null || type.TypeKind == TypeKind.Error) return null;
        }
        var raw = interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken)
            || interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
        var builder = new StringBuilder();
        var index = 0;
        foreach (var content in interpolated.Contents)
        {
            switch (content)
            {
                case InterpolatedStringTextSyntax text:
                    var value = text.TextToken.ValueText;
                    // Normal/verbatim tokens still contain doubled C# braces. Raw-string tokens
                    // contain literal braces. Recreate AppendLiteral, then composite-format escape.
                    if (!raw) value = value.Replace("{{", "{").Replace("}}", "}");
                    builder.Append(value.Replace("{", "{{").Replace("}", "}}"));
                    break;
                case InterpolationSyntax item:
                    builder.Append('{').Append(index++);
                    if (item.AlignmentClause != null)
                    {
                        var alignment = model.GetConstantValue(item.AlignmentClause.Value);
                        if (!alignment.HasValue || alignment.Value is not int number) return null;
                        builder.Append(',').Append(number.ToString(CultureInfo.InvariantCulture));
                    }
                    if (item.FormatClause != null) builder.Append(':').Append(item.FormatClause.FormatStringToken.ValueText);
                    builder.Append('}');
                    break;
                default: return null;
            }
        }
        return builder.ToString();
    }
}
