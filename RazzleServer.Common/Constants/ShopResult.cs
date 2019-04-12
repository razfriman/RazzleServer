namespace RazzleServer.Common.Constants
{
    public enum ShopResult : byte
    {
        BuySuccess = 0,
        BuyNoStock,
        BuyNoMoney,
        BuyUnknown,
        SellSuccess,
        SellNoStock,
        SellIncorrectRequest,
        SellUnkonwn,
        RechargeSuccess,
        RechargeNoStock,
        RechargeNoMoney,
        RechargeIncorrectRequest,
        RechargeUnknown
    }
}
