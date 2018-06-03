namespace RazzleServer.Common.Constants
{
    #region Quests

    public enum QuestResult : byte
    {
        AddTimeLimit = 0x06,
        RemoveTimeLimit = 0x07,
        Complete = 0x08,
        GenericError = 0x09,
        NoInventorySpace = 0x0A,
        NotEnoughMesos = 0x0B,
        ItemWornByChar = 0x0D,
        OnlyOneOfItemAllowed = 0x0E,
        Expire = 0x0F,
        ResetTimeLimit = 0x10
    }
    #endregion
}