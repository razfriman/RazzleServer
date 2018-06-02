namespace RazzleServer.Common.Constants
{
    public enum AdminCommandType : byte
    {
        CreateItem = 0,
        DestroyFirstItem = 1,
        GiveExperience = 2,
        Ban = 3,
        Block = 4,
        VarSetGet = 9,
        Hide = 16,
        ShowMessageMap = 17,
        Send = 18,
        Summon = 23,
        Snow = 28,
        Warn = 29,
        Log = 30,
        SetObjectState = 34
    }
}
