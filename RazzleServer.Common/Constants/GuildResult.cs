namespace RazzleServer.Common.Constants
{
    public enum GuildResult : byte
    {
        Create = 1,
        Invite = 5,
        ChangeEmblem = 17,
        Info = 26,
        AddMember = 39,
        InviteeNotInChannel = 40,
        InviteeAlreadyInGuild = 42,
        LeaveMember = 44,
        MemberExpel = 47,
        Disband = 50,
        MemberOnline = 61,
        UpdateRanks = 62,
        ChangeRank = 64,
        ShowEmblem = 66,
        UpdateNotice = 68
    }
}
