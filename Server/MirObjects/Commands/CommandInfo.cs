using System;

namespace Server.MirObjects
{
    /// <summary>
    /// Privilege needed to run a command. The values are ordered: a higher level may run every
    /// command of a lower level, so a game master can use everything a player can, and the
    /// reverse is not true. Do not turn this into bit flags: "GM includes player" is expressed
    /// by the comparison, not by setting both bits on every GM command.
    /// </summary>
    public enum CommandPermission
    {
        Player = 0,
        GameMaster = 1,
    }

    /// <summary>Groups commands for the command list page and any generated help output.</summary>
    public enum CommandCategory
    {
        General,
        Character,
        Items,
        Social,
        Guild,
        Quest,
        Movement,
        Monster,
        World,
        Admin,
    }

    /// <summary>
    /// One chat command. The registry is the single source of truth for dispatch, help output and
    /// the command list page, so a new command is declared once and shows up everywhere.
    /// </summary>
    public sealed class CommandInfo
    {
        public CommandInfo(string name, CommandPermission permission, CommandCategory category, string usage, string description)
        {
            Name = name;
            Permission = permission;
            Category = category;
            Usage = usage ?? string.Empty;
            Description = description ?? string.Empty;
        }

        /// <summary>Command name without the leading @.</summary>
        public string Name { get; }

        public CommandPermission Permission { get; }

        public CommandCategory Category { get; }

        /// <summary>Argument sketch shown after the name; empty when the command takes no arguments.</summary>
        public string Usage { get; }

        /// <summary>English source text. The language table translates it under ui.Command.NAME.</summary>
        public string Description { get; }

        /// <summary>True when a plain player may run this command on a test server.</summary>
        public bool TestServerAllowsPlayers { get; set; }

        /// <summary>SpecialItemMode that grants the command to players, empty when none does.</summary>
        public string SpecialItem { get; set; } = string.Empty;

        /// <summary>
        /// Runs the command. Null while a command has not migrated to the registry yet, which is
        /// how the dispatcher knows to fall back to the old switch during the migration.
        /// </summary>
        public Action<PlayerObject, string[]> Handler { get; set; }

        /// <summary>True when a plain player can reach the handler; the handler itself may still refuse.</summary>
        public bool AvailableToPlayers => Permission == CommandPermission.Player;

        /// <summary>The command as a player would type it.</summary>
        public string DisplayUsage => string.IsNullOrEmpty(Usage) ? "@" + Name : "@" + Name + " " + Usage;
    }
}
