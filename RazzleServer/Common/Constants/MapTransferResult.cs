namespace RazzleServer.Common.Constants
{
    public enum MapTransferResult : byte
    {
        NoReason = 0,
        PortalClosed = 1,
        CannotGo = 2,
        ForceOfGround = 3,
        CannotTeleport = 4,
        ForceOfGround2 = 5,
        OnlyByParty = 6,
        CashShopNotAvailable = 7
    }
}
