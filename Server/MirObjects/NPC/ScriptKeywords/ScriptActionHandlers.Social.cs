using System.Linq;

namespace Server.MirObjects
{
    public static partial class ScriptActionHandlers
    {
        // ---------------------------------------------------------------- name lists

        public static ScriptParseOutcome Act_AddNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.AddNameList, true);
        }

        public static ScriptParseOutcome Act_AddGuildNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.AddGuildNameList, true);
        }

        public static ScriptParseOutcome Act_DelNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.DelNameList, false);
        }

        public static ScriptParseOutcome Act_DelGuildNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.DelGuildNameList, false);
        }

        public static ScriptParseOutcome Act_ClearNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.ClearNameList, false);
        }

        public static ScriptParseOutcome Act_ClearGuildNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, ActionType.ClearGuildNameList, false);
        }

        // ---------------------------------------------------------------- guild and marriage

        public static ScriptParseOutcome Act_AddToGuild(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.AddToGuild, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RemoveFromGuild(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RemoveFromGuild, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_MakeWeddingRing(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.MakeWeddingRing));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ForceDivorce(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ForceDivorce));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- messages

        public static ScriptParseOutcome Act_LocalMessage(ScriptParseContext ctx)
        {
            return AddQuotedMessage(ctx, ActionType.LocalMessage);
        }

        public static ScriptParseOutcome Act_GlobalMessage(ScriptParseContext ctx)
        {
            return AddQuotedMessage(ctx, ActionType.GlobalMessage);
        }

        public static ScriptParseOutcome Act_OpenBrowser(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.OpenBrowser, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_PlaySound(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.PlaySound, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- mail

        public static ScriptParseOutcome Act_ComposeMail(ScriptParseContext ctx)
        {
            return AddQuotedMessage(ctx, ActionType.ComposeMail);
        }

        public static ScriptParseOutcome Act_AddMailGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.AddMailGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_AddMailItem(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.AddMailItem, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SendMail(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.SendMail));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- buffs

        public static ScriptParseOutcome Act_GiveBuff(ScriptParseContext ctx)
        {
            var buffParams = ctx.Parts.Skip(1).ToList();

            while (buffParams.Count < 5) buffParams.Add(string.Empty);

            ctx.Acts.Add(new NPCActions(ActionType.GiveBuff, buffParams.ToArray()));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RemoveBuff(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RemoveBuff, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }
    }
}
