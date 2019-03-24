namespace RazzleServer.Common.Packet
{
    public enum ClientOperationCode : byte
    {
        Login = 0x01,
        LoginSelectChannel = 0x02,
        SelectWorld = 0x03,
        SelectCharacter = 0x04,
        FieldEnter = 0x05,
        CheckName = 0x06,
        CreateCharacter = 0x07,
        DeleteCharacter = 0x08,
        Pong = 0x09,
        ClientCrashReport = 0x0A,
        ClientHash = 0x0E,

        FieldEnterPortal = 0x11,
        ChangeChannel = 0x12,
        FieldConnectCashShop = 0x13,
        FieldPlayerMovement = 0x14,
        FieldPlayerSitMapChair = 0x15,
        AttackMelee = 0x16,
        AttackRanged = 0x17,
        AttackMagic = 0x18,
        PlayerReceiveDamage = 0x1A,
        PlayerEmote = 0x1C,
        NpcSelect = 0x1F,

        NpcChat = 0x20,
        NpcOpenShop = 0x21,
        NpcStorage = 0x22,
        InventoryChangeSlot = 0x23,
        InventoryUseItem = 0x24,
        InventoryUseSummonSack = 0x25,
        InventoryUseCashItem = 0x27,
        InventoryUseReturnScroll = 0x28,
        InventoryUseScrollOnItem = 0x29,
        StatsChange = 0x2A,
        StatsHeal = 0x2B,
        SkillAddLevel = 0x2C,
        PlayerChat = 0x2D,
        SkillUse = 0x2D,
        SkillStop = 0x2E,

        InventoryDropMesos = 0x30,
        RemoteModifyFame = 0x31,
        RemoteRequestInfo = 0x32,
        TeleportRockUse = 0x37,
        CommandWhisperFind = 0x3C,

        SummonMove = 0x4E,
        SummonAttack = 0x4F,
        SummonDamage = 0x50,

        MobControl = 0x56,
        MobDistanceFromPlayer = 0x57,
        MobPickupDrop = 0x58,
        NpcAnimate = 0x5B,
        DropPickup = 0x5F,
        
        Unknown = 0xFF
    }
}
