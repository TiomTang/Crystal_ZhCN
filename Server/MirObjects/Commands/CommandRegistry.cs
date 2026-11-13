using System;
using System.Collections.Generic;

namespace Server.MirObjects
{
    /// <summary>
    /// Single source of truth for chat commands.
    /// <para>
    /// Adding a command means adding one entry here plus its handler; the command list page and
    /// any help output read this table, so nothing else has to be updated by hand. Permissions are
    /// declared here instead of being scattered through the dispatcher, which is what let commands
    /// such as @DIE and @SETTIMER ship without any check at all.
    /// </para>
    /// </summary>
    public static class CommandRegistry
    {
        private static readonly CommandInfo[] Entries =
        {
            new CommandInfo("LOGIN", CommandPermission.Player, CommandCategory.Admin, "",
                "Start GM authentication; the next chat line is read as the GM password."),
            new CommandInfo("KILL", CommandPermission.GameMaster, CommandCategory.Monster, "[player]",
                "Kill a player or the target in front of you.") { Handler = (player, args) => player.Cmd_KILL(args) },
            new CommandInfo("CHANGEGENDER", CommandPermission.GameMaster, CommandCategory.Character, "[player]",
                "Change the gender of yourself or a player.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CHANGEGENDER(args) },
            new CommandInfo("LEVEL", CommandPermission.GameMaster, CommandCategory.Character, "<level> [player]",
                "Set the level of yourself or a player.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_LEVEL(args) },
            new CommandInfo("LEVELHERO", CommandPermission.GameMaster, CommandCategory.Character, "<level> [player]",
                "Set the level of a player's hero.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_LEVELHERO(args) },
            new CommandInfo("MAKE", CommandPermission.GameMaster, CommandCategory.Items, "<item> [count] [player]",
                "Create an item by name or index.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_MAKE(args) },
            new CommandInfo("CLEARBUFFS", CommandPermission.Player, CommandCategory.Character, "",
                "Remove every buff from yourself."),
            new CommandInfo("CLEARBAG", CommandPermission.GameMaster, CommandCategory.Items, "[player]",
                "Empty a player's inventory.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CLEARBAG(args) },
            new CommandInfo("SUPERMAN", CommandPermission.GameMaster, CommandCategory.Admin, "",
                "Toggle GM invincibility.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_SUPERMAN(args) },
            new CommandInfo("GAMEMASTER", CommandPermission.GameMaster, CommandCategory.Admin, "",
                "Toggle GM mode.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_GAMEMASTER(args) },
            new CommandInfo("OBSERVER", CommandPermission.GameMaster, CommandCategory.Admin, "",
                "Toggle observer mode.") { Handler = (player, args) => player.Cmd_OBSERVER(args) },
            new CommandInfo("ALLOWGUILD", CommandPermission.Player, CommandCategory.Social, "",
                "Turn guild invitations on or off."),
            new CommandInfo("RECALL", CommandPermission.GameMaster, CommandCategory.Movement, "<player>",
                "Bring a player to your side.") { Handler = (player, args) => player.Cmd_RECALL(args) },
            new CommandInfo("OBSERVE", CommandPermission.Player, CommandCategory.Social, "<player>",
                "Observe a player; needs the target's permission unless you are a GM."),
            new CommandInfo("ENABLEGROUPRECALL", CommandPermission.Player, CommandCategory.Social, "",
                "Accept or refuse group recalls."),
            new CommandInfo("GROUPRECALL", CommandPermission.Player, CommandCategory.Social, "",
                "Group leader: pull every member to your side."),
            new CommandInfo("RECALLMEMBER", CommandPermission.Player, CommandCategory.Social, "<player>",
                "Group leader: pull one member to your side."),
            new CommandInfo("RECALLLOVER", CommandPermission.Player, CommandCategory.Social, "",
                "Pull your spouse to your side."),
            new CommandInfo("TIME", CommandPermission.Player, CommandCategory.General, "",
                "Show the server time."),
            new CommandInfo("ROLL", CommandPermission.Player, CommandCategory.Social, "",
                "Roll a dice for the group."),
            new CommandInfo("MAP", CommandPermission.Player, CommandCategory.General, "",
                "Show the current map name and identifier."),
            new CommandInfo("BACKUPPLAYER", CommandPermission.GameMaster, CommandCategory.Admin, "<player>",
                "Write a backup copy of a character.") { Handler = (player, args) => player.Cmd_BACKUPPLAYER(args) },
            new CommandInfo("ARCHIVEPLAYER", CommandPermission.GameMaster, CommandCategory.Admin, "<player>",
                "Archive a character.") { Handler = (player, args) => player.Cmd_ARCHIVEPLAYER(args) },
            new CommandInfo("LOADPLAYER", CommandPermission.GameMaster, CommandCategory.Admin, "<player>",
                "Load an archived character.") { Handler = (player, args) => player.Cmd_LOADPLAYER(args) },
            new CommandInfo("RESTOREPLAYER", CommandPermission.GameMaster, CommandCategory.Admin, "<character> [account]",
                "Restore an archived character onto an account.") { Handler = (player, args) => player.Cmd_RESTOREPLAYER(args) },
            new CommandInfo("MOVE", CommandPermission.GameMaster, CommandCategory.Movement, "<map> | <x> <y>",
                "Move to a map or to coordinates.") { TestServerAllowsPlayers = true, SpecialItem = "Teleport", Handler = (player, args) => player.Cmd_MOVE(args) },
            new CommandInfo("MAPMOVE", CommandPermission.GameMaster, CommandCategory.Movement, "<map> [instance] [x y]",
                "Send yourself or your group to a map by file name.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_MAPMOVE(args) },
            new CommandInfo("GOTO", CommandPermission.GameMaster, CommandCategory.Movement, "<player>",
                "Teleport next to a player.") { Handler = (player, args) => player.Cmd_GOTO(args) },
            new CommandInfo("MOB", CommandPermission.GameMaster, CommandCategory.Monster, "<monster> [count] [x y]",
                "Spawn monsters near you.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_MOB(args) },
            new CommandInfo("RECALLMOB", CommandPermission.GameMaster, CommandCategory.Monster, "<monster> [count]",
                "Spawn monsters as your pets.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_RECALLMOB(args) },
            new CommandInfo("RELOADDROPS", CommandPermission.GameMaster, CommandCategory.World, "",
                "Reload the drop configuration.") { Handler = (player, args) => player.Cmd_RELOADDROPS(args) },
            new CommandInfo("RELOADNPCS", CommandPermission.GameMaster, CommandCategory.World, "",
                "Reload NPC scripts.") { Handler = (player, args) => player.Cmd_RELOADNPCS(args) },
            new CommandInfo("CLEARIPBLOCKS", CommandPermission.GameMaster, CommandCategory.Admin, "",
                "Clear the blocked IP list.") { Handler = (player, args) => player.Cmd_CLEARIPBLOCKS(args) },
            new CommandInfo("GIVEGOLD", CommandPermission.GameMaster, CommandCategory.Items, "<amount> [player]",
                "Give gold.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_GIVEGOLD(args) },
            new CommandInfo("GIVEPEARLS", CommandPermission.GameMaster, CommandCategory.Items, "<amount> [player]",
                "Give pearls.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_GIVEPEARLS(args) },
            new CommandInfo("GIVECREDIT", CommandPermission.GameMaster, CommandCategory.Items, "<amount> [player]",
                "Give credit.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_GIVECREDIT(args) },
            new CommandInfo("GIVESKILL", CommandPermission.GameMaster, CommandCategory.Character, "<spell> [level] [player]",
                "Teach a spell.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_GIVESKILL(args) },
            new CommandInfo("FIND", CommandPermission.GameMaster, CommandCategory.Monster, "<monster>",
                "Find the nearest monster of a kind.") { SpecialItem = "Probe", Handler = (player, args) => player.Cmd_FIND(args) },
            new CommandInfo("LEAVEGUILD", CommandPermission.Player, CommandCategory.Guild, "",
                "Leave your current guild."),
            new CommandInfo("CREATEGUILD", CommandPermission.GameMaster, CommandCategory.Guild, "<name> [leader]",
                "Create a guild, skipping level, gold and newbie-name checks.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CREATEGUILD(args) },
            new CommandInfo("ALLOWTRADE", CommandPermission.Player, CommandCategory.Social, "",
                "Allow or refuse trade requests."),
            new CommandInfo("TRIGGER", CommandPermission.GameMaster, CommandCategory.World, "<key> [player]",
                "Run an NPC trigger page.") { Handler = (player, args) => player.Cmd_TRIGGER(args) },
            new CommandInfo("RIDE", CommandPermission.Player, CommandCategory.Character, "",
                "Mount or dismount."),
            new CommandInfo("SETFLAG", CommandPermission.GameMaster, CommandCategory.Quest, "<index> [value]",
                "Set a character flag.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_SETFLAG(args) },
            new CommandInfo("LISTFLAGS", CommandPermission.GameMaster, CommandCategory.Quest, "",
                "List the character flags that are set.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_LISTFLAGS(args) },
            new CommandInfo("CLEARFLAGS", CommandPermission.GameMaster, CommandCategory.Quest, "[player]",
                "Clear all character flags.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CLEARFLAGS(args) },
            new CommandInfo("CLEARMOB", CommandPermission.GameMaster, CommandCategory.Monster, "[map]",
                "Remove monsters from a map.") { Handler = (player, args) => player.Cmd_CLEARMOB(args) },
            new CommandInfo("CHANGECLASS", CommandPermission.GameMaster, CommandCategory.Character, "<class> [player]",
                "Change the class of yourself or a player.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CHANGECLASS(args) },
            new CommandInfo("DIE", CommandPermission.Player, CommandCategory.Character, "",
                "Kill yourself."),
            new CommandInfo("HAIR", CommandPermission.GameMaster, CommandCategory.Character, "[hair]",
                "Set the hair style.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_HAIR(args) },
            new CommandInfo("DECO", CommandPermission.GameMaster, CommandCategory.World, "<image> [x y]",
                "Place a decoration object.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_DECO(args) },
            new CommandInfo("ADJUSTPKPOINT", CommandPermission.GameMaster, CommandCategory.Character, "<points> [player]",
                "Adjust PK points.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_ADJUSTPKPOINT(args) },
            new CommandInfo("AWAKENING", CommandPermission.GameMaster, CommandCategory.Items, "<item type> <awake type> [player]",
                "Add an awakening to a worn item.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_AWAKENING(args) },
            new CommandInfo("REMOVEAWAKENING", CommandPermission.GameMaster, CommandCategory.Items, "<item type> [player]",
                "Remove an awakening from a worn item.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_REMOVEAWAKENING(args) },
            new CommandInfo("STARTWAR", CommandPermission.GameMaster, CommandCategory.Guild, "<guild>",
                "Start a guild war.") { Handler = (player, args) => player.Cmd_STARTWAR(args) },
            new CommandInfo("ADDINVENTORY", CommandPermission.Player, CommandCategory.Items, "",
                "Buy extra inventory rows with gold."),
            new CommandInfo("ADDSTORAGE", CommandPermission.Player, CommandCategory.Items, "",
                "Buy extra storage time with gold."),
            new CommandInfo("SUMMONHERO", CommandPermission.Player, CommandCategory.Character, "",
                "Summon or recall your hero."),
            new CommandInfo("ALLOWOBSERVE", CommandPermission.Player, CommandCategory.Social, "",
                "Allow or refuse being observed by other players."),
            new CommandInfo("INFO", CommandPermission.GameMaster, CommandCategory.Monster, "[target]",
                "Show detailed information about a monster or item.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_INFO(args) },
            new CommandInfo("CLEARQUESTS", CommandPermission.GameMaster, CommandCategory.Quest, "[player]",
                "Clear quest progress.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_CLEARQUESTS(args) },
            new CommandInfo("SETQUEST", CommandPermission.GameMaster, CommandCategory.Quest, "<quest> <state> [player]",
                "Set the state of a quest.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_SETQUEST(args) },
            new CommandInfo("TOGGLETRANSFORM", CommandPermission.Player, CommandCategory.Character, "",
                "Pause or resume a transform effect."),
            new CommandInfo("STARTCONQUEST", CommandPermission.GameMaster, CommandCategory.Guild, "<conquest id>",
                "Start a conquest.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_STARTCONQUEST(args) },
            new CommandInfo("RESETCONQUEST", CommandPermission.GameMaster, CommandCategory.Guild, "<conquest id>",
                "Reset a conquest.") { TestServerAllowsPlayers = true, Handler = (player, args) => player.Cmd_RESETCONQUEST(args) },
            new CommandInfo("GATES", CommandPermission.Player, CommandCategory.Guild, "",
                "Open or close conquest gates; needs guild conquest rights."),
            new CommandInfo("CHANGEFLAG", CommandPermission.Player, CommandCategory.Guild, "<flag>",
                "Change the guild conquest flag; needs guild conquest rights."),
            new CommandInfo("CHANGEFLAGCOLOUR", CommandPermission.Player, CommandCategory.Guild, "<colour>",
                "Change the guild flag colour; needs guild conquest rights."),
            new CommandInfo("REVIVE", CommandPermission.GameMaster, CommandCategory.Character, "[player]",
                "Revive yourself or a player.") { Handler = (player, args) => player.Cmd_REVIVE(args) },
            new CommandInfo("DELETESKILL", CommandPermission.GameMaster, CommandCategory.Character, "<spell> [player]",
                "Remove a spell.") { Handler = (player, args) => player.Cmd_DELETESKILL(args) },
            new CommandInfo("SETTIMER", CommandPermission.Player, CommandCategory.Quest, "<key> <seconds> <type>",
                "Set a quest timer."),
            new CommandInfo("SETLIGHT", CommandPermission.GameMaster, CommandCategory.Character, "<0-255>",
                "Set your light range.") { Handler = (player, args) => player.Cmd_SETLIGHT(args) },
        };

        public static IReadOnlyList<CommandInfo> All => Entries;

        public static int Count => Entries.Length;

        /// <summary>Looks a command up by name, ignoring case and a leading @.</summary>
        public static CommandInfo Find(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            if (name[0] == '@') name = name.Substring(1);

            foreach (CommandInfo entry in Entries)
            {
                if (string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase)) return entry;
            }

            return null;
        }

        /// <summary>True when the given privilege level may run the command.</summary>
        public static bool Allows(CommandInfo command, CommandPermission level)
        {
            return command != null && level >= command.Permission;
        }

        /// <summary>
        /// True when this player may run the command right now. A game master level always wins,
        /// then the two documented ways a plain player can borrow a GM command: a test server, or
        /// the special item named on the entry.
        /// </summary>
        public static bool Allows(PlayerObject player, CommandInfo command)
        {
            if (player == null || command == null) return false;
            if (command.Permission <= CommandPermission.Player) return true;
            if (player.IsGM) return true;
            if (command.TestServerAllowsPlayers && Settings.TestServer) return true;

            if (!string.IsNullOrEmpty(command.SpecialItem) &&
                Enum.TryParse(command.SpecialItem, true, out SpecialItemMode mode) &&
                player.SpecialMode.HasFlag(mode))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Runs the command through the registry. Returns false when the registry does not own this
        /// command yet, which lets the dispatcher fall back to the legacy switch while commands are
        /// still being migrated. A command that exists but is refused is consumed silently, the
        /// same way the old per-case guards behaved.
        /// </summary>
        public static bool TryDispatch(PlayerObject player, string[] parts)
        {
            if (player == null || parts == null || parts.Length == 0) return false;

            CommandInfo command = Find(parts[0]);

            if (command == null || command.Handler == null) return false;

            if (!Allows(player, command)) return true;

            command.Handler(player, parts);

            return true;
        }

        /// <summary>
        /// Self check for duplicates and half-declared entries. Returns one message per problem;
        /// an empty list means the table is consistent.
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (CommandInfo entry in Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    problems.Add("A command has no name.");
                    continue;
                }

                if (!seen.Add(entry.Name)) problems.Add("Duplicate command: " + entry.Name);
                if (string.IsNullOrWhiteSpace(entry.Description)) problems.Add("No description: " + entry.Name);
                if (!Enum.IsDefined(typeof(CommandPermission), entry.Permission)) problems.Add("Bad permission: " + entry.Name);
                if (!Enum.IsDefined(typeof(CommandCategory), entry.Category)) problems.Add("Bad category: " + entry.Name);
            }

            return problems;
        }
    }
}
