namespace RazzleServer.Common.Constants
{
    public enum NpcMessageType : byte
    {
        Standard,
        YesNo,
        RequestText,
        RequestNumber,
        Simple,
        Quiz = 6,
        RequestStyle = 7,
        AcceptDecline = 12,
        AcceptDeclineNoExit = 13
    }
}
