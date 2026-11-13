using System.Text.RegularExpressions;

namespace Server.MirObjects
{
    /// <summary>
    /// Handlers for the #ACT keyword table. The registry checks the minimum argument count before
    /// calling these, so a handler only deals with the keywords it owns.
    /// </summary>
    public static partial class ScriptActionHandlers
    {
        private static readonly Regex QuoteRegex = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);
        private static readonly Regex FlagRegex = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);
        private static readonly Regex VariableRegex = new Regex(@"[A-Z][0-9]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static ScriptParseOutcome AddNameList(ScriptParseContext ctx, ActionType action, bool createFile)
        {
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            string listPath = ctx.Parts[1];

            if (quoteMatch.Success)
                listPath = quoteMatch.Groups[1].Captures[0].Value;

            string fileName = Settings.GetNameListPath(listPath);
            string directory = System.IO.Path.GetDirectoryName(fileName);

            System.IO.Directory.CreateDirectory(directory);

            if (createFile && !System.IO.File.Exists(fileName))
                System.IO.File.Create(fileName).Close();

            ctx.Acts.Add(new NPCActions(action, fileName));

            return ScriptParseOutcome.Handled;
        }

        private static ScriptParseOutcome AddQuotedMessage(ScriptParseContext ctx, ActionType action)
        {
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            if (!quoteMatch.Success) return ScriptParseOutcome.Skipped;

            string message = quoteMatch.Groups[1].Captures[0].Value;

            ctx.Acts.Add(new NPCActions(action, message, ctx.Parts[ctx.Parts.Length - 1]));

            return ScriptParseOutcome.Handled;
        }
    }
}
