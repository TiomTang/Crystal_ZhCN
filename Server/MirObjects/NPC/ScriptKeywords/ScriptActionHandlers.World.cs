using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.MirObjects
{
    public static partial class ScriptActionHandlers
    {
        // ---------------------------------------------------------------- flow

        public static ScriptParseOutcome Act_Goto(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.Goto, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Call(ScriptParseContext ctx)
        {
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            string listPath = ctx.Parts[1];

            if (quoteMatch.Success)
                listPath = quoteMatch.Groups[1].Captures[0].Value;

            string fileName = Path.Combine(Settings.NPCPath, listPath + ".txt");

            if (!File.Exists(fileName))
            {
                ScriptDiagnostics.Report(ScriptDiagnosticKind.MissingScript, ctx, listPath,
                    "Called script not found: " + listPath);

                return ScriptParseOutcome.Skipped;
            }

            NPCScript script = NPCScript.GetOrAdd(0, listPath, NPCScriptType.Called);

            ctx.Segment.Page.ScriptCalls.Add(script.ScriptID);

            ctx.Acts.Add(new NPCActions(ActionType.Call, script.ScriptID.ToString()));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Break(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.Break));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Close(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.Close));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_DelayGoto(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.DelayGoto, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GroupGoto(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GroupGoto, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_EnterMap(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.EnterMap));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- timers

        public static ScriptParseOutcome Act_TimeRecall(ScriptParseContext ctx)
        {
            string page = ctx.Parts.Length > 2 ? ctx.Parts[2] : string.Empty;

            ctx.Acts.Add(new NPCActions(ActionType.TimeRecall, ctx.Parts[1], page));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TimeRecallGroup(ScriptParseContext ctx)
        {
            string page = ctx.Parts.Length > 2 ? ctx.Parts[2] : string.Empty;

            ctx.Acts.Add(new NPCActions(ActionType.TimeRecallGroup, ctx.Parts[1], page));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_BreakTimeRecall(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.BreakTimeRecall));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SetTimer(ScriptParseContext ctx)
        {
            string global = ctx.Parts.Length < 5 ? string.Empty : ctx.Parts[4];

            ctx.Acts.Add(new NPCActions(ActionType.SetTimer, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3], global));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ExpireTimer(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ExpireTimer, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- dice

        public static ScriptParseOutcome Act_RollDie(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RollDie, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RollYut(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RollYut, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GetRandomText(ScriptParseContext ctx)
        {
            Match match = VariableRegex.Match(ctx.Parts[2]);

            if (match.Success)
                ctx.Acts.Add(new NPCActions(ActionType.GetRandomText, ctx.Parts[1], ctx.Parts[2]));

            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- values

        public static ScriptParseOutcome Act_LoadValue(ScriptParseContext ctx)
        {
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            if (!quoteMatch.Success) return ScriptParseOutcome.Skipped;

            string fileName = Path.Combine(Settings.ValuePath, quoteMatch.Groups[1].Captures[0].Value);

            string group = ctx.Parts[ctx.Parts.Length - 2];
            string key = ctx.Parts[ctx.Parts.Length - 1];

            string directory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directory);

            if (!File.Exists(fileName))
                File.Create(fileName).Close();

            ctx.Acts.Add(new NPCActions(ActionType.LoadValue, ctx.Parts[1], fileName, group, key));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SaveValue(ScriptParseContext ctx)
        {
            MatchCollection matches = QuoteRegex.Matches(ctx.Line);

            if (matches.Count == 0 || !matches[0].Success) return ScriptParseOutcome.Skipped;

            string fileName = Path.Combine(Settings.ValuePath, matches[0].Groups[1].Captures[0].Value);

            string value = ctx.Parts[ctx.Parts.Length - 1];

            if (matches.Count > 1 && matches[1].Success)
                value = matches[1].Groups[1].Captures[0].Value;

            string[] newParts = ctx.Line.Replace(value, string.Empty).Replace("\"", string.Empty).Trim().Split(' ');

            string group = newParts[newParts.Length - 2];
            string key = newParts[newParts.Length - 1];

            string directory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directory);

            if (!File.Exists(fileName))
                File.Create(fileName).Close();

            ctx.Acts.Add(new NPCActions(ActionType.SaveValue, fileName, group, key, value));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- monsters and gates

        public static ScriptParseOutcome Act_MonGen(ScriptParseContext ctx)
        {
            string count = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];

            ctx.Acts.Add(new NPCActions(ActionType.Mongen, ctx.Parts[1], count));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_MonClear(ScriptParseContext ctx)
        {
            string instanceId = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];
            string monster = ctx.Parts.Length < 4 ? string.Empty : ctx.Parts[3];

            ctx.Acts.Add(new NPCActions(ActionType.MonClear, ctx.Parts[1], instanceId, monster));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_OpenGate(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.OpenGate, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_CloseGate(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.CloseGate, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- conquest

        public static ScriptParseOutcome Act_ConquestGuard(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ConquestGuard, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ConquestGate(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ConquestGate, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ConquestWall(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ConquestWall, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SetConquestRate(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.SetConquestRate, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_StartConquest(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.StartConquest, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ScheduleConquest(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ScheduleConquest, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ConquestRepairAll(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ConquestRepairAll, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- guild territory

        public static ScriptParseOutcome Act_BuyGT(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.BuyGT));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ExtendGT(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ExtendGT));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GTRecall(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GTRecall, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GTAllRecall(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GTAllRecall));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GTSale(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GTSale, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_CancelGTSale(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GTCancelSale));
            return ScriptParseOutcome.Handled;
        }
    }
}
