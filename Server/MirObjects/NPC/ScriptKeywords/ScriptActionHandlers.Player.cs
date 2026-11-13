using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.MirObjects
{
    public static partial class ScriptActionHandlers
    {
        // ---------------------------------------------------------------- movement

        public static ScriptParseOutcome Act_Move(ScriptParseContext ctx)
        {
            string x = ctx.Parts.Length > 2 ? ctx.Parts[2] : "0";
            string y = ctx.Parts.Length > 3 ? ctx.Parts[3] : "0";

            ctx.Acts.Add(new NPCActions(ActionType.Move, ctx.Parts[1], x, y));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_InstanceMove(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.InstanceMove, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3], ctx.Parts[4]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GroupRecall(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GroupRecall));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GroupTeleport(ScriptParseContext ctx)
        {
            string instanceId;
            string x;
            string y;

            if (ctx.Parts.Length == 4)
            {
                instanceId = "1";
                x = ctx.Parts[2];
                y = ctx.Parts[3];
            }
            else
            {
                instanceId = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];
                x = ctx.Parts.Length < 4 ? "0" : ctx.Parts[3];
                y = ctx.Parts.Length < 5 ? "0" : ctx.Parts[4];
            }

            ctx.Acts.Add(new NPCActions(ActionType.GroupTeleport, ctx.Parts[1], instanceId, x, y));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TeleportGT(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TeleportGT));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- currency

        public static ScriptParseOutcome Act_GiveGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakeGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TakeGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GiveGuildGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveGuildGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakeGuildGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TakeGuildGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GiveCredit(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveCredit, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakeCredit(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TakeCredit, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GivePearls(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GivePearls, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakePearls(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TakePearls, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakeConquestGold(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.TakeConquestGold, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- items

        public static ScriptParseOutcome Act_GiveItem(ScriptParseContext ctx)
        {
            string count = ctx.Parts.Length < 3 ? string.Empty : ctx.Parts[2];

            ctx.Acts.Add(new NPCActions(ActionType.GiveItem, ctx.Parts[1], count));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_TakeItem(ScriptParseContext ctx)
        {
            string count = ctx.Parts.Length < 3 ? string.Empty : ctx.Parts[2];
            string durability = ctx.Parts.Length > 3 ? ctx.Parts[3] : string.Empty;

            ctx.Acts.Add(new NPCActions(ActionType.TakeItem, ctx.Parts[1], count, durability));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_UnequipItem(ScriptParseContext ctx)
        {
            string type = ctx.Parts.Length >= 2 ? ctx.Parts[1] : string.Empty;

            ctx.Acts.Add(new NPCActions(ActionType.UnequipItem, type));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Drop(ScriptParseContext ctx)
        {
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            string listPath = ctx.Parts[1];

            if (quoteMatch.Success)
                listPath = quoteMatch.Groups[1].Captures[0].Value;

            string fileName = Path.Combine(Settings.DropPath, listPath);

            ctx.Acts.Add(new NPCActions(ActionType.Drop, fileName));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- player state

        public static ScriptParseOutcome Act_GiveExp(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveExp, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GiveHP(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveHP, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_GiveMP(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.GiveMP, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ChangeLevel(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ChangeLevel, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SetPkPoint(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.SetPkPoint, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ReducePkPoint(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ReducePkPoint, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_IncreasePkPoint(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.IncreasePkPoint, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ChangeGender(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ChangeGender));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ChangeClass(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ChangeClass, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ChangeHair(ScriptParseContext ctx)
        {
            if (ctx.Parts.Length < 2)
                ctx.Acts.Add(new NPCActions(ActionType.ChangeHair));
            else
                ctx.Acts.Add(new NPCActions(ActionType.ChangeHair, ctx.Parts[1]));

            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Set(ScriptParseContext ctx)
        {
            Match match = FlagRegex.Match(ctx.Parts[1]);

            if (match.Success)
            {
                string flagIndex = match.Groups[1].Captures[0].Value;
                ctx.Acts.Add(new NPCActions(ActionType.Set, flagIndex, ctx.Parts[2]));
            }

            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Param1(ScriptParseContext ctx)
        {
            string instanceId = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];

            ctx.Acts.Add(new NPCActions(ActionType.Param1, ctx.Parts[1], instanceId));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Param2(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.Param2, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Param3(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.Param3, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Mov(ScriptParseContext ctx)
        {
            Match match = VariableRegex.Match(ctx.Parts[1]);
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            string valueToStore = ctx.Parts[2];

            if (quoteMatch.Success)
                valueToStore = quoteMatch.Groups[1].Captures[0].Value;

            if (match.Success)
                ctx.Acts.Add(new NPCActions(ActionType.Mov, ctx.Parts[1], valueToStore));

            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_Calc(ScriptParseContext ctx)
        {
            Match match = VariableRegex.Match(ctx.Parts[1]);
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            string valueToStore = ctx.Parts[3];

            if (quoteMatch.Success)
                valueToStore = quoteMatch.Groups[1].Captures[0].Value;

            if (match.Success)
                ctx.Acts.Add(new NPCActions(ActionType.Calc, "%" + ctx.Parts[1], ctx.Parts[2], valueToStore, ctx.Parts[1].Insert(1, "-")));

            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RefreshEffects(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RefreshEffects));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_CanGainExp(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.CanGainExp, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- skills

        public static ScriptParseOutcome Act_GiveSkill(ScriptParseContext ctx)
        {
            string spellLevel = ctx.Parts.Length > 2 ? ctx.Parts[2] : "0";

            ctx.Acts.Add(new NPCActions(ActionType.GiveSkill, ctx.Parts[1], spellLevel));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RemoveSkill(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RemoveSkill, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- pets

        public static ScriptParseOutcome Act_GivePet(ScriptParseContext ctx)
        {
            string petCount = ctx.Parts.Length > 2 ? ctx.Parts[2] : "1";
            string petLevel = ctx.Parts.Length > 3 ? ctx.Parts[3] : "0";

            ctx.Acts.Add(new NPCActions(ActionType.GivePet, ctx.Parts[1], petCount, petLevel));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_RemovePet(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.RemovePet, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_ClearPets(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ClearPets));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- hero

        public static ScriptParseOutcome Act_ReviveHero(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.ReviveHero));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_SealHero(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.SealHero));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_DeleteHero(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.DeleteHero));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_HeroGiveSkill(ScriptParseContext ctx)
        {
            string spellLevel = ctx.Parts.Length > 2 ? ctx.Parts[2] : "0";

            ctx.Acts.Add(new NPCActions(ActionType.HeroGiveSkill, ctx.Parts[1], spellLevel));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Act_HeroRemoveSkill(ScriptParseContext ctx)
        {
            ctx.Acts.Add(new NPCActions(ActionType.HeroRemoveSkill, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }
    }
}
