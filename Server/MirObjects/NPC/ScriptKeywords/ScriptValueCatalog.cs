namespace Server.MirObjects
{
    /// <summary>
    /// The &lt;$VALUE&gt; table used inside #SAY. Variables are not dispatched, so they carry no
    /// handler; the table exists for validation and for the editor's insertion list.
    /// </summary>
    public static class ScriptValueCatalog
    {
        /// <summary>Values available when the script runs for a player.</summary>
        public static readonly ScriptKeyword[] PlayerEntries =
        {
            new ScriptKeyword("MONSTERCOUNT()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(map)",
                "Number of monsters on a map.", 1, null),

            new ScriptKeyword("CONQUESTGUARD()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index, guard index)",
                "Name and repair cost of a conquest guard.", 1, null),

            new ScriptKeyword("CONQUESTGATE()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index, gate index)",
                "Name and repair cost of a conquest gate.", 1, null),

            new ScriptKeyword("CONQUESTWALL()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index, wall index)",
                "Name and repair cost of a conquest wall.", 1, null),

            new ScriptKeyword("CONQUESTSIEGE()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index, gate index)",
                "Name and repair cost of a siege gate.", 1, null),

            new ScriptKeyword("CONQUESTOWNER()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index)",
                "Guild that owns a conquest.", 1, null),

            new ScriptKeyword("CONQUESTGOLD()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index)",
                "Gold stored by a conquest.", 1, null),

            new ScriptKeyword("CONQUESTRATE()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index)",
                "NPC rate of a conquest.", 1, null),

            new ScriptKeyword("CONQUESTSCHEDULE()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(conquest index)",
                "War scheduled for a conquest.", 1, null),

            new ScriptKeyword("OUTPUT()", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "(variable)",
                "Value of a script variable.", 1, null),

            new ScriptKeyword("NPCNAME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the NPC the player is talking to.", 1, null),

            new ScriptKeyword("USERNAME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the player.", 1, null),

            new ScriptKeyword("LEVEL", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Player level.", 1, null),

            new ScriptKeyword("CLASS", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Player class name.", 1, null),

            new ScriptKeyword("MAP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current map file name.", 1, null),

            new ScriptKeyword("X_COORD", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current map X coordinate.", 1, null),

            new ScriptKeyword("Y_COORD", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current map Y coordinate.", 1, null),

            new ScriptKeyword("HP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current hit points.", 1, null),

            new ScriptKeyword("MAXHP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Maximum hit points.", 1, null),

            new ScriptKeyword("MP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current mana.", 1, null),

            new ScriptKeyword("MAXMP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Maximum mana.", 1, null),

            new ScriptKeyword("GAMEGOLD", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Gold the player carries.", 1, null),

            new ScriptKeyword("CREDIT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Credit the player holds.", 1, null),

            new ScriptKeyword("ARMOUR", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the equipped armour.", 1, null),

            new ScriptKeyword("WEAPON", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the equipped weapon.", 1, null),

            new ScriptKeyword("RING_L", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the left ring.", 1, null),

            new ScriptKeyword("RING_R", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the right ring.", 1, null),

            new ScriptKeyword("BRACELET_L", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the left bracelet.", 1, null),

            new ScriptKeyword("BRACELET_R", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the right bracelet.", 1, null),

            new ScriptKeyword("NECKLACE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the necklace.", 1, null),

            new ScriptKeyword("BELT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the belt.", 1, null),

            new ScriptKeyword("BOOTS", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the boots.", 1, null),

            new ScriptKeyword("HELMET", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the helmet.", 1, null),

            new ScriptKeyword("AMULET", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the amulet.", 1, null),

            new ScriptKeyword("STONE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the equipped stone, or the empty text.", 1, null),

            new ScriptKeyword("TORCH", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the equipped torch, or the empty text.", 1, null),

            new ScriptKeyword("DATE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current server date.", 1, null),

            new ScriptKeyword("MAPLIGHT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current map light setting.", 1, null),

            new ScriptKeyword("USERCOUNT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Number of players online.", 1, null),

            new ScriptKeyword("PKPOINT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Player PK points.", 1, null),

            new ScriptKeyword("GUILDWARTIME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Length of a guild war.", 1, null),

            new ScriptKeyword("GUILDWARFEE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Cost of starting a guild war.", 1, null),

            new ScriptKeyword("PARCELAMOUNT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Number of parcels waiting to be collected.", 1, null),

            new ScriptKeyword("GUILDNAME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the player's guild.", 1, null),

            new ScriptKeyword("ROLLRESULT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Result of the last die roll.", 1, null),

            new ScriptKeyword("MOUNTLOYALTY", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Loyalty of the equipped mount.", 1, null),

            new ScriptKeyword("MOUNT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the equipped mount.", 1, null),

            new ScriptKeyword("GUILDEXTENDFEE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Cost of extending the guild territory.", 1, null),

            new ScriptKeyword("GUILDRENTFEE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Rent of the guild territory.", 1, null),

            new ScriptKeyword("GUILDGTRENTALDAYSLEFT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Days left on the guild territory rental.", 1, null),

            new ScriptKeyword("AGITGUILDNAME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Guild that owns the territory.", 1, null),
        };

        /// <summary>Values available when the script runs for a monster.</summary>
        public static readonly ScriptKeyword[] MonsterEntries =
        {
            new ScriptKeyword("USERNAME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Name of the monster.", 1, null),

            new ScriptKeyword("LEVEL", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Monster level.", 1, null),

            new ScriptKeyword("MAP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Map file name the monster stands on.", 1, null),

            new ScriptKeyword("X_COORD", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Map X coordinate of the monster.", 1, null),

            new ScriptKeyword("Y_COORD", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Map Y coordinate of the monster.", 1, null),

            new ScriptKeyword("HP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current hit points of the monster.", 1, null),

            new ScriptKeyword("MAXHP", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Maximum hit points of the monster.", 1, null),

            new ScriptKeyword("DATE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current server date.", 1, null),

            new ScriptKeyword("MAPLIGHT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Current map light setting.", 1, null),

            new ScriptKeyword("USERCOUNT", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Number of players online.", 1, null),

            new ScriptKeyword("GUILDWARTIME", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Length of a guild war.", 1, null),

            new ScriptKeyword("GUILDWARFEE", ScriptKeywordKind.Variable, ScriptKeywordCategory.Variable, "",
                "Cost of starting a guild war.", 1, null),
        };
    }
}
