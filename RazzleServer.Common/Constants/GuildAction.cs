namespace RazzleServer.Common.Constants
{
    #region Social

    public enum GuildAction : byte
    {
        Update = 0,
        Create = 2,
        Invite = 5,
        Join = 6,
        Leave = 7,
        Expel = 8,
        ModifyTitles = 13,
        ModifyRank = 14,
        ModifyEmblem = 15,
        ModifyNotice = 16
    }
    #endregion
}