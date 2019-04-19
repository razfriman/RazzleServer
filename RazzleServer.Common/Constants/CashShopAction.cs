namespace RazzleServer.Common.Constants
{
    public enum CashShopAction : byte
    {
        ClientBuyItem = 2,
        ClientGiftItem = 3,
        ClientUpdateWishList = 4,
        ClinetIncreaseSlots = 5,
        ClientMoveToStorage = 10,
        ClientMoveToLocer = 11,


        ServerLoadLocker = 28,
        ServerLoadLockerFailed = 29,
        ServerLoadWishList = 30,
        ServerLoadWishListFailed = 31,
        ServerUpdateWishList = 32,
        ServerUpdateWishListFAiled = 33,
        ServerBuy = 34,
        ServerBuyFailed = 35,
        ServerGift = 41,
        ServerGiftFailed = 42,
        ServerIncreaseSlot = 43,
        ServerIncreaseSlotFailed = 44,
        ServerIncreaseTrunk = 45,
        ServerIncreaseTrunkFailed = 46,
        ServerMoveToStorage = 47,
        ServerMoveToStorageFailed = 48,
        ServerMoveToLocker = 49,
        ServerMoveToLockerFailed = 50,
        ServerDelete = 51,
        ServerDeleteFailed = 52,
        ServerExpired = 53,
        ServerExpiredFailed = 54,
        ServerGiftPackage = 72
    }
}
