namespace Server.MirObjects
{
    public enum GuildHomeTeleportResult
    {
        Success,
        NotInGuild,
        NotOwner,
        WarInProgress,
        PalaceUnavailable
    }

    // Guild home teleport is limited to the ShaBaKe palace (map index 245).
    // Every failure must be reported to the caller so the item is never consumed.
    public static class GuildHomeTeleportRules
    {
        public const int ShaBaKePalaceMapIndex = 245;

        public static GuildHomeTeleportResult Evaluate(GuildObject guild)
        {
            if (guild == null) return GuildHomeTeleportResult.NotInGuild;

            ConquestObject conquest = guild.Conquest;

            if (conquest == null || conquest.Guild != guild) return GuildHomeTeleportResult.NotOwner;
            if (conquest.WarIsOn) return GuildHomeTeleportResult.WarInProgress;
            if (conquest.Info == null || conquest.Info.PalaceIndex != ShaBaKePalaceMapIndex) return GuildHomeTeleportResult.PalaceUnavailable;
            if (conquest.PalaceMap == null) return GuildHomeTeleportResult.PalaceUnavailable;

            return GuildHomeTeleportResult.Success;
        }
    }
}
