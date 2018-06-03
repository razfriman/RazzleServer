namespace RazzleServer.Common.Constants
{
    public enum NpcMessageType : byte
    {
        Standard,
        YesNo,
        RequestText,
        RequestNumber,
        Choice,
        RequestStyle = 7,
        AcceptDecline = 12
    }
}
