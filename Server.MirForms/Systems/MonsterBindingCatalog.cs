using Server.MirEnvir;

namespace Server
{
    /// <summary>
    /// The [Game] monster name references: which setting key points at which monster record, the
    /// group the key belongs to and the code path that consumes it.
    /// <para>
    /// Values are not cached here. They are always read through
    /// <see cref="Settings.GetMonsterNameSettings"/> and written through
    /// <see cref="Settings.SetMonsterNameSettings"/>, so this list cannot drift from the fields it
    /// describes. FishingMonster is deliberately absent: it is owned by the fishing page, which
    /// stores it in FishingSystem.ini and shadows the Setup.ini value.
    /// </para>
    /// </summary>
    internal static class MonsterBindingCatalog
    {
        internal const string Section = "Game";

        internal const string GroupTaoist = "Taoist summons";
        internal const string GroupClones = "Clones";
        internal const string GroupPoison = "Poison and totem summons";
        internal const string GroupStone = "Stone trap";
        internal const string GroupBoss = "Boss minion summons";
        internal const string GroupSpecial = "Special spawns";

        internal sealed class Definition
        {
            internal Definition(string key, string group, string usedBy)
            {
                Key = key;
                Group = group;
                UsedBy = usedBy;
            }

            /// <summary>Setting key inside [Game].</summary>
            internal string Key { get; }

            /// <summary>Purpose group shown in the editor.</summary>
            internal string Group { get; }

            /// <summary>Class that resolves this key at runtime.</summary>
            internal string UsedBy { get; }
        }

        private static readonly Definition[] Definitions =
        {
            new Definition("SkeletonName", GroupTaoist, "HumanObject"),
            new Definition("ShinsuName", GroupTaoist, "HumanObject"),
            new Definition("AngelName", GroupTaoist, "HumanObject"),

            new Definition("CloneName", GroupClones, "HumanObject"),
            new Definition("AssassinCloneName", GroupClones, "HumanObject"),

            new Definition("VampireName", GroupPoison, "HumanObject"),
            new Definition("ToadName", GroupPoison, "HumanObject"),
            new Definition("SnakeTotemName", GroupPoison, "HumanObject"),
            new Definition("SnakesName", GroupPoison, "SnakeTotem"),

            new Definition("StoneName", GroupStone, "HumanObject"),

            new Definition("Zuma1", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma2", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma3", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma4", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma5", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma6", GroupBoss, "ZumaTaurus"),
            new Definition("Zuma7", GroupBoss, "ZumaTaurus"),
            new Definition("Turtle1", GroupBoss, "TurtleKing"),
            new Definition("Turtle2", GroupBoss, "TurtleKing"),
            new Definition("Turtle3", GroupBoss, "TurtleKing"),
            new Definition("Turtle4", GroupBoss, "TurtleKing"),
            new Definition("Turtle5", GroupBoss, "TurtleKing"),
            new Definition("BoneMonster1", GroupBoss, "BoneLord"),
            new Definition("BoneMonster2", GroupBoss, "BoneLord"),
            new Definition("BoneMonster3", GroupBoss, "BoneLord"),
            new Definition("BoneMonster4", GroupBoss, "BoneLord"),
            new Definition("BehemothMonster1", GroupBoss, "Behemoth"),
            new Definition("BehemothMonster2", GroupBoss, "Behemoth"),
            new Definition("BehemothMonster3", GroupBoss, "Behemoth"),
            new Definition("HellKnight1", GroupBoss, "HellLord"),
            new Definition("HellKnight2", GroupBoss, "HellLord"),
            new Definition("HellKnight3", GroupBoss, "HellLord"),
            new Definition("HellKnight4", GroupBoss, "HellLord"),
            new Definition("HellBomb1", GroupBoss, "HellLord"),
            new Definition("HellBomb2", GroupBoss, "HellLord"),
            new Definition("HellBomb3", GroupBoss, "HellLord"),
            new Definition("GeneralMeowMeowMob1", GroupBoss, "GeneralMeowMeow"),
            new Definition("GeneralMeowMeowMob2", GroupBoss, "GeneralMeowMeow"),
            new Definition("GeneralMeowMeowMob3", GroupBoss, "GeneralMeowMeow"),
            new Definition("GeneralMeowMeowMob4", GroupBoss, "GeneralMeowMeow"),
            new Definition("KingHydraxMob", GroupBoss, "KingHydrax"),
            new Definition("HornedCommanderMob", GroupBoss, "HornedCommander"),
            new Definition("HornedCommanderBombMob", GroupBoss, "HornedCommander"),
            new Definition("SnowWolfKingMob", GroupBoss, "SnowWolfKing"),
            new Definition("ScrollMob1", GroupBoss, "HoodedSummoner"),
            new Definition("ScrollMob2", GroupBoss, "HoodedSummoner"),
            new Definition("ScrollMob3", GroupBoss, "HoodedSummoner"),
            new Definition("ScrollMob4", GroupBoss, "HoodedSummoner"),
            new Definition("WhiteSnake", GroupBoss, "Yimoogi"),

            new Definition("BugBatName", GroupSpecial, "BugBagMaggot"),
            new Definition("BombSpiderName", GroupSpecial, "RootSpider"),
            new Definition("AncientBatName", GroupSpecial, "AncientBringer"),
            new Definition("TucsonGeneralEgg", GroupSpecial, "TucsonEgg")
        };

        internal static IReadOnlyList<Definition> All => Definitions;
    }
}
