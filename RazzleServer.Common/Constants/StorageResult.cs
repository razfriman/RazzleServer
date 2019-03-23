namespace RazzleServer.Common.Constants
{
    public enum StorageResult : byte
    {
        RemoveItem = 0x07,
        InventoryFullOrNot = 0x08,
        AddItem = 0x09,
        NotEnoughMesos = 0x0B,
        StorageIsFull = 0x0C,
        ChangeMeso = 0x0E
    }
}
