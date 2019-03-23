namespace RazzleServer.Common.Packet
{
    public enum ServerOperationCode : ushort
    {
        CheckPasswordResult = 0x01,
        ClientConnectToServerLogin = 0x05,
        LoginCharacterRemoveResult = 0x08,
        ClientConnectToServer = 0x09,
        Ping = 0x0A,

        InventoryChangeSlot = 0x12,
        InventoryChangeInventorySlots = 0x13,
        StatsChange = 0x14,
        SkillsGiveBuff = 0x15,
        SkillsGiveDebuff = 0x16,
        SkillsAddPoint = 0x17,
        Fame = 0x19,
        Notice = 0x1A,
        TeleportRock = 0x1C,
        PlayerInformation = 0x1F,

        PartyOperation = 0x20,
        BuddyOperation = 0x21,
        Message = 0x23,
        EnterMap = 0x26,
        IncorrectChannelNumber = 0x2B,
        SlashCmdAnswer = 0x2E,

        RemotePlayerSpawn = 0x3C,
        RemotePlayerDespawn = 0x3D,
        RemotePlayerChat = 0x3F,

        SummonMove = 0x4B, // Wrong?
        SummonDespawn = 0x4B, // Wrong?
        SummonAttack = 0x4D,
        SummonDamage = 0x4E,

        RemotePlayerMove = 0x52,
        RemotePlayerMeleeAttack = 0x53,
        RemotePlayerRangedAttack = 0x54,
        RemotePlayerMagicAttack = 0x55,
        RemotePlayerGetDamage = 0x58,
        RemotePlayerEmote = 0x59,
        RemotePlayerChangeEquips = 0x5A,
        RemotePlayerAnimation = 0x5B,
        RemotePlayerSkillBuff = 0x5C,
        RemotePlayerSkillDebuff = 0x5D,

        RemotePlayerSitOnChair = 0x61,
        RemotePlayerThirdPartyAnimation = 0x62,
        CashShopUnavailable = 0x64,
        MesoSackResult = 0x65,
        MobSpawn = 0x6A,
        MobRespawn = 0x6B,
        MobControlRequest = 0x6C,
        MobMovement = 0x6E,
        MobControlResponse = 0x6F,

        MobChangeHealth = 0x75,
        NpcSpawn = 0x7B,
        NpcControlRequest = 0x7D,
        NpcAnimate = 0x7F,

        DropSpawn = 0x83,
        DropModify = 0x84,

        ReactorHit = 0x94,
        ReactorSpawn = 0x96,
        ReactorDestroy = 0x97,
        SnowBallState = 0x9A,
        SnowBallHit = 0x9B,
        CoconutHit = 0x9C,
        CoconutScore = 0x9D,

        NpcScriptChat = 0xA0,
        NpcShopShow = 0xA3,
        NpcShopResult = 0xA4,
        StorageShow = 0xA7,
        StorageResult = 0xA8
    }
}
