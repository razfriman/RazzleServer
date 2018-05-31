namespace RazzleServer.Common.Packet
{
    public enum ClientOperationCode : ushort
    {
        LeaveCharacterSelect = 0,
        SelectCharacterByVac = 0,
        VacFlagSet = 0,

        Unknown = 0x00,
        AccountLogin = 0x01,
        GuestLogin = 0x02,
        WorldRelist = 0x04,
        WorldSelect = 0x05,
        WorldStatus = 0x06,
        AccountGender = 0x08,
        PinCheck = 0x09,
        RegisterPin = 0x0A,
        WorldList = 0x0B,
        ViewAllCharacters = 0x0D,
        ViewAllCharactersSelect = 0x0E,
        SelectCharacter = 0x13,
        CharacterLoad = 0x14,
        CharacterNameCheck = 0x15,
        CharacterCreate = 0x16,
        CharacterDelete = 0x17,

        Pong = 0x18,
        StrangeData = 0x1A,
        Relogin = 0x1C,

        ChangeMap = 0x23,
        ChannelChange = 0x24,
        CashShopMigration = 0x25,
        [IgnorePacketPrint] PlayerMovement = 0x26,
        Sit = 0x27,
        ChairUse = 0x28,
        CloseRangeAttack = 0x29,
        RangedAttack = 0x2A,
        MagicAttack = 0x2B,
        EnergyOrbAttack = 0x2C,
        TakeDamage = 0x2D,
        PlayerChat = 0x2E,
        ChalkboardClose = 0x2F,

        FaceExpression = 0x30,
        UseItemEffect = 0x31,
        NpcResult = 0x36,
        NpcConverse = 0x38,
        NpcShop = 0x39,
        Storage = 0x3A,
        HiredMerchant = 0x3B,
        DueyOperation = 0x3D,
        ItemSort = 0x40,
        ItemSort2 = 0x41,
        InventoryAction = 0x42,
        UseItem = 0x43,
        CancelItemEffect = 0x44,
        UseSummonBag = 0x46,
        UsePetFood = 0x47,
        UseMountFood = 0x48,
        UseCashItem = 0x49,
        UseCatchItem = 0x4A,
        UseSkillBook = 0x4B,
        UseReturnScroll = 0x4E,
        UseUpgradeScroll = 0x4F,

        DistributeAp = 0x50,
        HealOverTime = 0x51,
        DistributeSp = 0x52,
        // /0x53
        CancelBuff = 0x54,
        SkillEffect = 0x55,
        MesoDrop = 0x56,
        GiveFame = 0x57,
        // 0x58
        PlayerInformation = 0x59,
        PetEnterField = 0x5A,
        CancelDebuff = 0x5B,
        TeleportRockUse = 0x5C,
        UseInnerPortal = 0x5D,
        TeleportRockOperation = 0x5E,
        // 0x5F

        QuestAction = 0x62,
        SkillMacro = 0x65,
        Report = 0x68,
        PartyChat = 0x6B,
        Whisper = 0x6C,
        SpouseChat = 0x6D,
        MultiChat = 0x6E,
        PlayerInteraction = 0x6F,

        PartyOperation = 0x70,
        DenyPartyRequest = 0x71,
        GuildOperation = 0x72,
        DenyGuildRequest = 0x73,
        BuddyListModify = 0x76,
        MemoOperation = 0x77,
        UseDoor = 0x79,
        ChangeKeymap = 0x7B,
        RingAction = 0x7D,

        AllianceOperation = 0x83,
        BbsOperation = 0x86,
        MtsMigration = 0x87,
        UseSolomonItem = 0x89,
        PetChat = 0x8B,
        PetMove = 0x8C,
        PetTalk = 0x8D,
        PetCommand = 0x8E,
        PetLoot = 0x8F,

        PetAutoPot = 0x90,
        SummonMove = 0x94,
        SummonAttack = 0x95,
        SummonDamage = 0x96,
        [IgnorePacketPrint] MobMovement = 0x9D,
        MobAutomaticProvoke = 0x9E,

        MobDamageModFriendly = 0xA0, // Guess
        MobDamageMob = 0xA1,
        MonsterBomb = 0xA2,
        NpcAction = 0xA6,
        DropPickup = 0xAB,
        HitReactor = 0xAE,
        TouchReactor = 0xAF, // Guess
        DamageReactor = 0xB0, // Guess

        MonsterCarnival = 0xB9,
        PartySearchStart = 0xBD,
        PartySearchRegister = 0xBF,

        PlayerUpdate = 0xC0,
        CashShopOperation = 0xC5,
        BuyCashItem = 0xC6,
        CouponCode = 0xC7,

        MapleTv = 0xD4,
        MtsOperation = 0xD9,

        Messenger = 0x3D, // Unknown
        EnterPortal = 0x47, // Unknown
        UseDeathItem = 0xFF, // Unknown
        AdminShopAction = 68, // Unknown
        UseSkill = 0x5B, // Unknown
        UseRemote = 0x73, // Unknown
        Command = 120, // Unknown
        AdminCommand = 128, // Unknown
        AdminLog = 129, // Unknown 
        NpcMovement = 197, // Unknown
    }
}