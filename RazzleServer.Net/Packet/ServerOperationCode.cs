﻿namespace RazzleServer.Net.Packet
{
    public enum ServerOperationCode : byte
    {
        CheckPasswordResult = 0x01,
        WorldStatus = 0x02,
        WorldInformation = 0x03,
        CharacterList = 0x04,
        ClientConnectToServerLogin = 0x05,
        CheckNameResult = 0x06,
        CreateCharacterResult = 0x07,
        DeleteCharacterResult = 0x08,
        ClientConnectToServer = 0x09,
        Ping = 0x0A,
        //Unknown = 0x0B,
        //Unknown = 0x0C,
        //Unknown = 0x0D,
        //Unknown = 0x0E,
        //Unknown = 0x0F,

        //Unknown = 0x10,
        //Unknown = 0x11,
        InventoryOperation = 0x12,
        InventoryChangeInventorySlots = 0x13,
        StatsChanged = 0x14,
        SkillsGiveBuff = 0x15,
        SkillsGiveDebuff = 0x16,
        SkillsAddPoint = 0x17,

        //Unknown = 0x18,
        Fame = 0x19,
        Message = 0x1A,

        //Unknown = 0x1B,
        TeleportRock = 0x1C,
        SueCharacterResult = 0x1D,

        //Unknown = 0x1E,
        PlayerInformation = 0x1F,

        PartyOperation = 0x20,
        BuddyOperation = 0x21,
        PortalEnterField = 0x22,
        Notice = 0x23,

        //Unknown = 0x24,
        //Unknown = 0x25,
        SetField = 0x26,
        SetFieldCashShop = 0x27,

        //Unknown = 0x28,
        //Unknown = 0x29,
        TransferFieldReqIgnored = 0x2A,
        IncorrectChannelNumber = 0x2B,

        //Unknown = 0x2C,
        GroupMessage = 0x2D,
        Whisper = 0x2E,
        //Unknown = 0x2F,

        MapEffect = 0x30, // Portal Effect, Boss HP
        WeatherEffect = 0x31,
        JukeboxEffect = 0x32,
        AdminResult = 0x33,
        GmEventInstructions = 0x35,
        MapClock = 0x36,
        Boat = 0x38,
        RemotePlayerEnterField = 0x3C,
        RemotePlayerLeaveField = 0x3D,
        RemotePlayerChat = 0x3F,

        AnnounceBox = 0x40,
        SummonMove = 0x4B,
        SummonLeaveField = 0x4B,
        SummonAttack = 0x4D,
        SummonDamage = 0x4E,

        RemotePlayerMove = 0x52,
        RemotePlayerMeleeAttack = 0x53,
        RemotePlayerRangedAttack = 0x54,
        RemotePlayerMagicAttack = 0x55,
        RemotePlayerGetDamage = 0x58,
        RemotePlayerEmote = 0x59,
        RemotePlayerChangeEquips = 0x5A,
        RemotePlayerEffect = 0x5B,
        RemotePlayerSkillBuff = 0x5C,
        RemotePlayerSkillDebuff = 0x5D,

        ItemEffect = 0x60,
        RemotePlayerSitOnChair = 0x61,
        Effect = 0x62, // RemotePlayerThirdPartyAnimation, SendPlayerSkillAnimThirdParty
        CashShopUnavailable = 0x64,
        MesoSackResult = 0x65,
        MobEnterField = 0x6A,
        MobLeaveField = 0x6B,
        MobControlRequest = 0x6C,
        MobMove = 0x6E,
        MobControlResponse = 0x6F,

        MobChangeHealth = 0x75,
        NpcEnterField = 0x7B,
        NpcLeaveField = 0x7C, // Guess
        NpcControlRequest = 0x7D,
        NpcMove = 0x7F,

        DropEnterField = 0x83,
        DropLeaveField = 0x84,
        KiteMessage = 0x87,
        KiteEnterField = 0x88,
        KiteLeaveField = 0x89,
        MistEnterField = 0x8C,
        MistLeaveField = 0x8D,

        DoorEnterField = 0x90,
        DoorLeaveField = 0x91,
        ReactorChangeState = 0x94,
        ReactorEnterField = 0x96,
        ReactorLeaveField = 0x97,
        SnowBallState = 0x9A,
        SnowBallHit = 0x9B,
        CoconutHit = 0x9C,
        CoconutScore = 0x9D,

        NpcScriptChat = 0xA0,
        NpcShopShow = 0xA3,
        NpcShopResult = 0xA4,
        StorageShow = 0xA7,
        StorageResult = 0xA8,
        PlayerInteraction = 0xAE,
        
        CashShopAmounts = 0xBA,
        CashShopOperation = 0xBB
    }
}
