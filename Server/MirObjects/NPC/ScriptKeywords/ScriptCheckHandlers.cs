using System.IO;
using System.Text.RegularExpressions;

namespace Server.MirObjects
{
    /// <summary>
    /// Handlers for the #IF keyword table. The registry checks the minimum argument count before
    /// calling these, so each handler can read the parts it needs without repeating that guard.
    /// </summary>
    public static class ScriptCheckHandlers
    {
        private static readonly Regex FlagRegex = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);
        private static readonly Regex QuoteRegex = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

        // ---------------------------------------------------------------- player

        public static ScriptParseOutcome Check_Level(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.Level, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Gold(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckGold, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Credit(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckCredit, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Gender(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckGender, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Class(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckClass, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_PkPoint(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckPkPoint, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Flag(ScriptParseContext ctx)
        {
            Match match = FlagRegex.Match(ctx.Parts[1]);

            if (match.Success)
            {
                string flagIndex = match.Groups[1].Captures[0].Value;
                ctx.Segment.CheckList.Add(new NPCChecks(CheckType.Check, flagIndex, ctx.Parts[2]));
            }

            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_IsNewHuman(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.IsNewHuman));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_IsAdmin(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.IsAdmin));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- items

        public static ScriptParseOutcome Check_Item(ScriptParseContext ctx)
        {
            string count = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];
            string durability = ctx.Parts.Length > 3 ? ctx.Parts[3] : string.Empty;

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckItem, ctx.Parts[1], count, durability));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_BagSpace(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.HasBagSpace, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- name lists

        public static ScriptParseOutcome Check_NameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, CheckType.CheckNameList);
        }

        public static ScriptParseOutcome Check_GuildNameList(ScriptParseContext ctx)
        {
            return AddNameList(ctx, CheckType.CheckGuildNameList);
        }

        private static ScriptParseOutcome AddNameList(ScriptParseContext ctx, CheckType type)
        {
            string listPath = ctx.Parts[1];
            Match quoteMatch = QuoteRegex.Match(ctx.Line);

            if (quoteMatch.Success)
                listPath = quoteMatch.Groups[1].Captures[0].Value;

            string fileName = Settings.GetNameListPath(listPath);
            string directory = Path.GetDirectoryName(fileName);

            Directory.CreateDirectory(directory);

            ctx.Segment.CheckList.Add(new NPCChecks(type, fileName));

            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- social

        public static ScriptParseOutcome Check_Relationship(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckRelationship));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_WeddingRing(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckWeddingRing));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- guild

        public static ScriptParseOutcome Check_GuildGold(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckGuildGold, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_InGuild(ScriptParseContext ctx)
        {
            string guildName = string.Empty;

            if (ctx.Parts.Length > 1) guildName = ctx.Parts[1];

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.InGuild, guildName));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Permission(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckPermission, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_HasGT(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.HasGT));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_AffordGuard(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.AffordGuard, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_AffordGate(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.AffordGate, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_AffordWall(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.AffordWall, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_AffordSiege(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.AffordSiege, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- group

        public static ScriptParseOutcome Check_GroupLeader(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.Groupleader));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_GroupCount(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.GroupCount, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_GroupNearby(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.GroupCheckNearby));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- pets

        public static ScriptParseOutcome Check_PetCount(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.PetCount, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_PetLevel(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.PetLevel, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Pet(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckPet, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- hero

        public static ScriptParseOutcome Check_HeroLevel(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.HeroLevel, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_HeroClass(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckHeroClass, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_HeroGender(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckHeroGender, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_HeroItem(ScriptParseContext ctx)
        {
            string count = ctx.Parts.Length < 3 ? "1" : ctx.Parts[2];
            string durability = ctx.Parts.Length > 3 ? ctx.Parts[3] : string.Empty;

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckHeroItem, ctx.Parts[1], count, durability));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- quests

        public static ScriptParseOutcome Check_Quest(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckQuest, ctx.Parts[1], ctx.Parts[2]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- world

        public static ScriptParseOutcome Check_Range(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckRange, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Map(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckMap, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_MapLight(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckMapLight, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Hum(ScriptParseContext ctx)
        {
            string instance = ctx.Parts.Length < 5 ? "1" : ctx.Parts[4];

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckHum, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3], instance));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Mon(ScriptParseContext ctx)
        {
            string instance = ctx.Parts.Length < 5 ? "1" : ctx.Parts[4];

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckMon, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3], instance));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_ExactMon(ScriptParseContext ctx)
        {
            string instance = ctx.Parts.Length < 6 ? "1" : ctx.Parts[5];

            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckExactMon, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3], ctx.Parts[4], instance));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Conquest(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckConquest, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_ConquestAvailable(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.ConquestAvailable, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_ConquestOwner(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.ConquestOwner, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- time

        public static ScriptParseOutcome Check_DayOfWeek(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckDay, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Hour(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckHour, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Minute(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckMinute, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Timer(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckTimer, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- buffs

        public static ScriptParseOutcome Check_Buff(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckBuff, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        // ---------------------------------------------------------------- flow

        public static ScriptParseOutcome Check_Random(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.Random, ctx.Parts[1]));
            return ScriptParseOutcome.Handled;
        }

        public static ScriptParseOutcome Check_Calc(ScriptParseContext ctx)
        {
            ctx.Segment.CheckList.Add(new NPCChecks(CheckType.CheckCalc, ctx.Parts[1], ctx.Parts[2], ctx.Parts[3]));
            return ScriptParseOutcome.Handled;
        }
    }
}
