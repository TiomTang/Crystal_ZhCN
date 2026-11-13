namespace Server.MirObjects
{
    /// <summary>
    /// The #ACT keyword table. Every entry names the handler that implements it, so the table doubles
    /// as the dispatch map and as the list the editor shows inside an #ACT or #ELSEACT block.
    /// </summary>
    public static class ScriptActionCatalog
    {
        public static readonly ScriptKeyword[] Entries =
        {
            new ScriptKeyword("MOVE", ScriptKeywordKind.Action, ScriptKeywordCategory.Movement, "<map> [x] [y]",
                "Move the player to a map position.", 2, ScriptActionHandlers.Act_Move),

            new ScriptKeyword("INSTANCEMOVE", ScriptKeywordKind.Action, ScriptKeywordCategory.Movement, "<map> <instance> <x> <y>",
                "Move the player to a map instance position.", 5, ScriptActionHandlers.Act_InstanceMove),

            new ScriptKeyword("GIVEGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Give gold to the player.", 2, ScriptActionHandlers.Act_GiveGold),

            new ScriptKeyword("TAKEGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Take gold from the player.", 2, ScriptActionHandlers.Act_TakeGold),

            new ScriptKeyword("GIVEGUILDGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Give gold to the player's guild.", 2, ScriptActionHandlers.Act_GiveGuildGold),

            new ScriptKeyword("TAKEGUILDGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Take gold from the player's guild.", 2, ScriptActionHandlers.Act_TakeGuildGold),

            new ScriptKeyword("GIVECREDIT", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Give credit to the player.", 2, ScriptActionHandlers.Act_GiveCredit),

            new ScriptKeyword("TAKECREDIT", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Take credit from the player.", 2, ScriptActionHandlers.Act_TakeCredit),

            new ScriptKeyword("GIVEPEARLS", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Give pearls to the player.", 2, ScriptActionHandlers.Act_GivePearls),

            new ScriptKeyword("TAKEPEARLS", ScriptKeywordKind.Action, ScriptKeywordCategory.Currency, "<amount>",
                "Take pearls from the player.", 2, ScriptActionHandlers.Act_TakePearls),

            new ScriptKeyword("GIVEITEM", ScriptKeywordKind.Action, ScriptKeywordCategory.Item, "<item> [count]",
                "Give an item to the player.", 2, ScriptActionHandlers.Act_GiveItem),

            new ScriptKeyword("TAKEITEM", ScriptKeywordKind.Action, ScriptKeywordCategory.Item, "<item> <count> [durability]",
                "Take an item from the player.", 3, ScriptActionHandlers.Act_TakeItem),

            new ScriptKeyword("GIVEEXP", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<amount>",
                "Give experience to the player.", 2, ScriptActionHandlers.Act_GiveExp),

            new ScriptKeyword("GIVEPET", ScriptKeywordKind.Action, ScriptKeywordCategory.Pet, "<pet> [count] [level]",
                "Give a pet to the player.", 2, ScriptActionHandlers.Act_GivePet),

            new ScriptKeyword("REMOVEPET", ScriptKeywordKind.Action, ScriptKeywordCategory.Pet, "<pet>",
                "Remove a pet from the player.", 2, ScriptActionHandlers.Act_RemovePet),

            new ScriptKeyword("CLEARPETS", ScriptKeywordKind.Action, ScriptKeywordCategory.Pet, "",
                "Remove every pet the player owns.", 1, ScriptActionHandlers.Act_ClearPets),

            new ScriptKeyword("GOTO", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<page>",
                "Jump to another page of this script.", 2, ScriptActionHandlers.Act_Goto),

            new ScriptKeyword("CALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<script>",
                "Run a page of another script.", 2, ScriptActionHandlers.Act_Call),

            new ScriptKeyword("BREAK", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "",
                "Stop processing the rest of the page.", 1, ScriptActionHandlers.Act_Break),

            new ScriptKeyword("CLOSE", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "",
                "Close the NPC dialog.", 1, ScriptActionHandlers.Act_Close),

            new ScriptKeyword("ADDNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Add the player's name to a name list.", 2, ScriptActionHandlers.Act_AddNameList),

            new ScriptKeyword("ADDGUILDNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Add the player's guild to a name list.", 2, ScriptActionHandlers.Act_AddGuildNameList),

            new ScriptKeyword("DELNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Remove the player's name from a name list.", 2, ScriptActionHandlers.Act_DelNameList),

            new ScriptKeyword("DELGUILDNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Remove the player's guild from a name list.", 2, ScriptActionHandlers.Act_DelGuildNameList),

            new ScriptKeyword("CLEARNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Empty a name list.", 2, ScriptActionHandlers.Act_ClearNameList),

            new ScriptKeyword("CLEARGUILDNAMELIST", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<list>",
                "Empty a guild name list.", 2, ScriptActionHandlers.Act_ClearGuildNameList),

            new ScriptKeyword("GIVEHP", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<amount>",
                "Restore hit points.", 2, ScriptActionHandlers.Act_GiveHP),

            new ScriptKeyword("GIVEMP", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<amount>",
                "Restore mana.", 2, ScriptActionHandlers.Act_GiveMP),

            new ScriptKeyword("CHANGELEVEL", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<level>",
                "Set the player's level.", 2, ScriptActionHandlers.Act_ChangeLevel),

            new ScriptKeyword("SETPKPOINT", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<points>",
                "Set the player's PK points.", 2, ScriptActionHandlers.Act_SetPkPoint),

            new ScriptKeyword("REDUCEPKPOINT", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<points>",
                "Reduce the player's PK points.", 2, ScriptActionHandlers.Act_ReducePkPoint),

            new ScriptKeyword("INCREASEPKPOINT", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<points>",
                "Increase the player's PK points.", 2, ScriptActionHandlers.Act_IncreasePkPoint),

            new ScriptKeyword("CHANGEGENDER", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "",
                "Switch the player's gender.", 1, ScriptActionHandlers.Act_ChangeGender),

            new ScriptKeyword("CHANGECLASS", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<class>",
                "Change the player's class.", 2, ScriptActionHandlers.Act_ChangeClass),

            new ScriptKeyword("CHANGEHAIR", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "[hair]",
                "Change the player's hair style.", 1, ScriptActionHandlers.Act_ChangeHair),

            new ScriptKeyword("LOCALMESSAGE", ScriptKeywordKind.Action, ScriptKeywordCategory.Message, "\"<text>\" [channel]",
                "Show a message to players near the NPC.", 1, ScriptActionHandlers.Act_LocalMessage),

            new ScriptKeyword("GLOBALMESSAGE", ScriptKeywordKind.Action, ScriptKeywordCategory.Message, "\"<text>\" [channel]",
                "Show a message to every player.", 1, ScriptActionHandlers.Act_GlobalMessage),

            new ScriptKeyword("GIVESKILL", ScriptKeywordKind.Action, ScriptKeywordCategory.Skill, "<skill> [level]",
                "Teach the player a skill.", 3, ScriptActionHandlers.Act_GiveSkill),

            new ScriptKeyword("REMOVESKILL", ScriptKeywordKind.Action, ScriptKeywordCategory.Skill, "<skill>",
                "Remove a skill from the player.", 2, ScriptActionHandlers.Act_RemoveSkill),

            new ScriptKeyword("SET", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "[<flag index>] <true|false>",
                "Set a player flag.", 3, ScriptActionHandlers.Act_Set),

            new ScriptKeyword("PARAM1", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<value> [instance]",
                "Store the first page parameter.", 2, ScriptActionHandlers.Act_Param1),

            new ScriptKeyword("PARAM2", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<value>",
                "Store the second page parameter.", 2, ScriptActionHandlers.Act_Param2),

            new ScriptKeyword("PARAM3", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<value>",
                "Store the third page parameter.", 2, ScriptActionHandlers.Act_Param3),

            new ScriptKeyword("MONGEN", ScriptKeywordKind.Action, ScriptKeywordCategory.World, "<monster> [count]",
                "Spawn monsters on the current map.", 2, ScriptActionHandlers.Act_MonGen),

            new ScriptKeyword("TIMERECALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<seconds> [page]",
                "Move the player after a delay.", 2, ScriptActionHandlers.Act_TimeRecall),

            new ScriptKeyword("TIMERECALLGROUP", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<seconds> [page]",
                "Move the group after a delay.", 2, ScriptActionHandlers.Act_TimeRecallGroup),

            new ScriptKeyword("BREAKTIMERECALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "",
                "Cancel a pending recall.", 1, ScriptActionHandlers.Act_BreakTimeRecall),

            new ScriptKeyword("DELAYGOTO", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<seconds> <page>",
                "Go to a page after a delay.", 3, ScriptActionHandlers.Act_DelayGoto),

            new ScriptKeyword("MONCLEAR", ScriptKeywordKind.Action, ScriptKeywordCategory.World, "<map> [instance] [monster]",
                "Remove monsters from a map.", 2, ScriptActionHandlers.Act_MonClear),

            new ScriptKeyword("GROUPRECALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Movement, "",
                "Bring every group member to the player.", 1, ScriptActionHandlers.Act_GroupRecall),

            new ScriptKeyword("GROUPTELEPORT", ScriptKeywordKind.Action, ScriptKeywordCategory.Movement, "<map> [instance] [x] [y]",
                "Move the group to a map position.", 2, ScriptActionHandlers.Act_GroupTeleport),

            new ScriptKeyword("MOV", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<variable> <value>",
                "Store a value in a script variable.", 3, ScriptActionHandlers.Act_Mov),

            new ScriptKeyword("CALC", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<variable> <operator> <value>",
                "Combine a script variable with a value.", 4, ScriptActionHandlers.Act_Calc),

            new ScriptKeyword("GIVEBUFF", ScriptKeywordKind.Action, ScriptKeywordCategory.Buff, "<buff> <seconds> [value]",
                "Give the player a buff.", 3, ScriptActionHandlers.Act_GiveBuff),

            new ScriptKeyword("REMOVEBUFF", ScriptKeywordKind.Action, ScriptKeywordCategory.Buff, "<buff>",
                "Remove a buff from the player.", 2, ScriptActionHandlers.Act_RemoveBuff),

            new ScriptKeyword("ADDTOGUILD", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<guild>",
                "Add the player to a guild.", 2, ScriptActionHandlers.Act_AddToGuild),

            new ScriptKeyword("REMOVEFROMGUILD", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "<guild>",
                "Remove the player from a guild.", 2, ScriptActionHandlers.Act_RemoveFromGuild),

            new ScriptKeyword("REFRESHEFFECTS", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "",
                "Refresh the player's visible effects.", 1, ScriptActionHandlers.Act_RefreshEffects),

            new ScriptKeyword("CANGAINEXP", ScriptKeywordKind.Action, ScriptKeywordCategory.Player, "<true|false>",
                "Allow or block experience gain.", 2, ScriptActionHandlers.Act_CanGainExp),

            new ScriptKeyword("COMPOSEMAIL", ScriptKeywordKind.Action, ScriptKeywordCategory.Mail, "\"<subject>\" [sender]",
                "Start composing a mail.", 1, ScriptActionHandlers.Act_ComposeMail),

            new ScriptKeyword("ADDMAILGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Mail, "<amount>",
                "Attach gold to the composed mail.", 2, ScriptActionHandlers.Act_AddMailGold),

            new ScriptKeyword("ADDMAILITEM", ScriptKeywordKind.Action, ScriptKeywordCategory.Mail, "<item> <count>",
                "Attach an item to the composed mail.", 3, ScriptActionHandlers.Act_AddMailItem),

            new ScriptKeyword("SENDMAIL", ScriptKeywordKind.Action, ScriptKeywordCategory.Mail, "",
                "Send the composed mail.", 1, ScriptActionHandlers.Act_SendMail),

            new ScriptKeyword("GROUPGOTO", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<page>",
                "Send the whole group to a page.", 2, ScriptActionHandlers.Act_GroupGoto),

            new ScriptKeyword("ENTERMAP", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "",
                "Enter the map chosen in the map dialog.", 1, ScriptActionHandlers.Act_EnterMap),

            new ScriptKeyword("MAKEWEDDINGRING", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "",
                "Create a wedding ring for the couple.", 1, ScriptActionHandlers.Act_MakeWeddingRing),

            new ScriptKeyword("FORCEDIVORCE", ScriptKeywordKind.Action, ScriptKeywordCategory.Social, "",
                "Divorce the player from their partner.", 1, ScriptActionHandlers.Act_ForceDivorce),

            new ScriptKeyword("LOADVALUE", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<variable> \"<file>\" <group> <key>",
                "Read a stored value into a variable.", 5, ScriptActionHandlers.Act_LoadValue),

            new ScriptKeyword("SAVEVALUE", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<variable> \"<file>\" <group> <key>",
                "Write a variable into the value file.", 5, ScriptActionHandlers.Act_SaveValue),

            new ScriptKeyword("CONQUESTGUARD", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest> <guard>",
                "Repair a conquest guard.", 3, ScriptActionHandlers.Act_ConquestGuard),

            new ScriptKeyword("CONQUESTGATE", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest> <gate>",
                "Repair a conquest gate.", 3, ScriptActionHandlers.Act_ConquestGate),

            new ScriptKeyword("CONQUESTWALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest> <wall>",
                "Repair a conquest wall.", 3, ScriptActionHandlers.Act_ConquestWall),

            new ScriptKeyword("TAKECONQUESTGOLD", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<amount>",
                "Take gold from the conquest treasury.", 2, ScriptActionHandlers.Act_TakeConquestGold),

            new ScriptKeyword("SETCONQUESTRATE", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<rate> <value>",
                "Set a conquest rate.", 3, ScriptActionHandlers.Act_SetConquestRate),

            new ScriptKeyword("STARTCONQUEST", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest>",
                "Start a conquest war.", 2, ScriptActionHandlers.Act_StartConquest),

            new ScriptKeyword("SCHEDULECONQUEST", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest>",
                "Schedule a conquest war.", 2, ScriptActionHandlers.Act_ScheduleConquest),

            new ScriptKeyword("OPENGATE", ScriptKeywordKind.Action, ScriptKeywordCategory.World, "<conquest> <gate>",
                "Open a conquest gate.", 3, ScriptActionHandlers.Act_OpenGate),

            new ScriptKeyword("CLOSEGATE", ScriptKeywordKind.Action, ScriptKeywordCategory.World, "<conquest> <gate>",
                "Close a conquest gate.", 3, ScriptActionHandlers.Act_CloseGate),

            new ScriptKeyword("OPENBROWSER", ScriptKeywordKind.Action, ScriptKeywordCategory.Message, "<url>",
                "Open a web page for the player.", 2, ScriptActionHandlers.Act_OpenBrowser),

            new ScriptKeyword("GETRANDOMTEXT", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<variable> <file>",
                "Store a random line of a file in a variable.", 3, ScriptActionHandlers.Act_GetRandomText),

            new ScriptKeyword("PLAYSOUND", ScriptKeywordKind.Action, ScriptKeywordCategory.Message, "<sound>",
                "Play a sound for the player.", 2, ScriptActionHandlers.Act_PlaySound),

            new ScriptKeyword("SETTIMER", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<key> <seconds> <type> [global]",
                "Start or update a timer.", 4, ScriptActionHandlers.Act_SetTimer),

            new ScriptKeyword("EXPIRETIMER", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<key>",
                "Expire a timer.", 2, ScriptActionHandlers.Act_ExpireTimer),

            new ScriptKeyword("UNEQUIPITEM", ScriptKeywordKind.Action, ScriptKeywordCategory.Item, "[slot]",
                "Unequip an item slot.", 1, ScriptActionHandlers.Act_UnequipItem),

            new ScriptKeyword("ROLLDIE", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<page> [autoroll]",
                "Roll a six-sided die.", 3, ScriptActionHandlers.Act_RollDie),

            new ScriptKeyword("ROLLYUT", ScriptKeywordKind.Action, ScriptKeywordCategory.Flow, "<page> [autoroll]",
                "Play a round of yut.", 3, ScriptActionHandlers.Act_RollYut),

            new ScriptKeyword("DROP", ScriptKeywordKind.Action, ScriptKeywordCategory.Item, "<drop file>",
                "Drop items from a drop file.", 2, ScriptActionHandlers.Act_Drop),

            new ScriptKeyword("REVIVEHERO", ScriptKeywordKind.Action, ScriptKeywordCategory.Hero, "",
                "Revive the player's hero.", 1, ScriptActionHandlers.Act_ReviveHero),

            new ScriptKeyword("SEALHERO", ScriptKeywordKind.Action, ScriptKeywordCategory.Hero, "",
                "Seal the player's hero.", 1, ScriptActionHandlers.Act_SealHero),

            new ScriptKeyword("DELETEHERO", ScriptKeywordKind.Action, ScriptKeywordCategory.Hero, "",
                "Delete the player's hero.", 1, ScriptActionHandlers.Act_DeleteHero),

            new ScriptKeyword("CONQUESTREPAIRALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Conquest, "<conquest>",
                "Repair every structure of a conquest.", 2, ScriptActionHandlers.Act_ConquestRepairAll),

            new ScriptKeyword("BUYGT", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "",
                "Buy the guild territory.", 1, ScriptActionHandlers.Act_BuyGT),

            new ScriptKeyword("TELEPORTGT", ScriptKeywordKind.Action, ScriptKeywordCategory.Movement, "",
                "Teleport to the guild territory.", 1, ScriptActionHandlers.Act_TeleportGT),

            new ScriptKeyword("EXTENDGT", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "",
                "Extend the territory rental.", 1, ScriptActionHandlers.Act_ExtendGT),

            new ScriptKeyword("GTRECALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "<player>",
                "Summon a guild member to the territory.", 2, ScriptActionHandlers.Act_GTRecall),

            new ScriptKeyword("GTALLRECALL", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "",
                "Summon every guild member to the territory.", 1, ScriptActionHandlers.Act_GTAllRecall),

            new ScriptKeyword("GTSALE", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "<price>",
                "Offer the territory for sale.", 2, ScriptActionHandlers.Act_GTSale),

            new ScriptKeyword("CANCELGTSALE", ScriptKeywordKind.Action, ScriptKeywordCategory.Guild, "",
                "Cancel the territory sale.", 1, ScriptActionHandlers.Act_CancelGTSale),

            new ScriptKeyword("HEROGIVESKILL", ScriptKeywordKind.Action, ScriptKeywordCategory.Hero, "<skill> [level]",
                "Teach the hero a skill.", 3, ScriptActionHandlers.Act_HeroGiveSkill),

            new ScriptKeyword("HEROREMOVESKILL", ScriptKeywordKind.Action, ScriptKeywordCategory.Hero, "<skill>",
                "Remove a skill from the hero.", 2, ScriptActionHandlers.Act_HeroRemoveSkill),
        };
    }
}
