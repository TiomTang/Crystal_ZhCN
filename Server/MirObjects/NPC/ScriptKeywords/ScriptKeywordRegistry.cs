using System;
using System.Collections.Generic;

namespace Server.MirObjects
{
    /// <summary>
    /// Dispatch table for NPC script keywords.
    /// <para>
    /// Each kind of keyword has one explicit table. Matching is a dictionary lookup, so there is no
    /// reflection and no second list to maintain: the table entry is the definition.
    /// </para>
    /// </summary>
    public static class ScriptKeywordRegistry
    {
        private static readonly Dictionary<string, ScriptKeyword> CheckIndex = BuildIndex(ScriptCheckCatalog.Entries);
        private static readonly Dictionary<string, ScriptKeyword> ActionIndex = BuildIndex(ScriptActionCatalog.Entries);

        private static readonly ScriptKeyword[] NoKeywords = new ScriptKeyword[0];

        /// <summary>Keywords allowed inside #IF.</summary>
        public static IReadOnlyList<ScriptKeyword> Checks
        {
            get { return ScriptCheckCatalog.Entries; }
        }

        /// <summary>Keywords allowed inside #ACT and #ELSEACT.</summary>
        public static IReadOnlyList<ScriptKeyword> Actions
        {
            get { return ScriptActionCatalog.Entries; }
        }

        /// <summary>Variables replaced inside #SAY, written as &lt;$NAME&gt;.</summary>
        public static IReadOnlyList<ScriptKeyword> Variables
        {
            get { return NoKeywords; }
        }

        public static bool TryGetCheck(string name, out ScriptKeyword keyword)
        {
            return CheckIndex.TryGetValue(name, out keyword);
        }

        public static bool TryGetAction(string name, out ScriptKeyword keyword)
        {
            return ActionIndex.TryGetValue(name, out keyword);
        }

        /// <summary>
        /// Parses one line against the table for its kind. Unrecognised keywords and incomplete lines
        /// are recorded in <see cref="ScriptDiagnostics"/> instead of being dropped silently.
        /// </summary>
        public static ScriptParseOutcome Dispatch(ScriptKeywordKind kind, ScriptParseContext context)
        {
            if (context == null || context.Parts == null || context.Parts.Length == 0) return ScriptParseOutcome.Skipped;

            Dictionary<string, ScriptKeyword> index = IndexFor(kind);

            if (index == null) return ScriptParseOutcome.Unknown;

            ScriptKeyword keyword;

            if (!index.TryGetValue(context.Parts[0], out keyword))
            {
                ScriptDiagnostics.Report(KindOf(kind), context, context.Parts[0],
                    string.Format("Unknown {0} \"{1}\"", KindName(kind), context.Parts[0]));

                return ScriptParseOutcome.Unknown;
            }

            if (context.Parts.Length < keyword.MinParts)
            {
                ScriptDiagnostics.Report(ScriptDiagnosticKind.WrongArity, context, keyword.Name,
                    string.Format("\"{0}\" needs at least {1} argument(s)", keyword.Name, keyword.MinParts - 1));

                return ScriptParseOutcome.Skipped;
            }

            return keyword.Handler(context);
        }

        private static Dictionary<string, ScriptKeyword> IndexFor(ScriptKeywordKind kind)
        {
            switch (kind)
            {
                case ScriptKeywordKind.Check:
                    return CheckIndex;
                case ScriptKeywordKind.Action:
                    return ActionIndex;
                default:
                    return null;
            }
        }

        private static ScriptDiagnosticKind KindOf(ScriptKeywordKind kind)
        {
            switch (kind)
            {
                case ScriptKeywordKind.Check:
                    return ScriptDiagnosticKind.UnknownCheck;
                case ScriptKeywordKind.Action:
                    return ScriptDiagnosticKind.UnknownAction;
                default:
                    return ScriptDiagnosticKind.UnknownValue;
            }
        }

        private static string KindName(ScriptKeywordKind kind)
        {
            switch (kind)
            {
                case ScriptKeywordKind.Check:
                    return "check";
                case ScriptKeywordKind.Action:
                    return "action";
                default:
                    return "variable";
            }
        }

        private static Dictionary<string, ScriptKeyword> BuildIndex(IEnumerable<ScriptKeyword> entries)
        {
            var index = new Dictionary<string, ScriptKeyword>(StringComparer.OrdinalIgnoreCase);

            foreach (ScriptKeyword entry in entries)
            {
                if (index.ContainsKey(entry.Name))
                {
#if DEBUG
                    throw new InvalidOperationException("Duplicate NPC script keyword: " + entry.Name);
#else
                    continue;
#endif
                }

                index.Add(entry.Name, entry);
            }

            return index;
        }
    }
}
