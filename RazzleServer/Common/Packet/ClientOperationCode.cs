namespace RazzleServer.Common.Packet
{
    public enum ClientOperationCode : ushort
    {
        LeaveCharacterSelect = 0,
        SelectCharacterByVAC = 0,
        VACFlagSet = 0,
        ClientError = 0,
        CharacterSelectRegisterPic = 0,
        CharacterSelectRequestPic = 0,
        RegisterPicFromVAC = 0,
        RequestPicFromVAC = 0,
        AccountInfo = 0,

        Unknown = 0x00,
        PinCheck = 0x03,
        WorldRelist = 0x04,
        RegisterPin = 0x05,
        ViewAllChar = 0x06,
        CharacterNameCheck = 0x09,
        Pong = 0x0A,
        CharacterCreate = 0x0E,
        CharacterDelete = 0x0F,

        StrangeData = 0x11,
        WorldStatus = 0x13,
        CharacterLoad = 0x14,
        SelectCharacter = 0x16,
        AccountGender = 0x17,
        WorldList = 0x18,
        WorldSelect = 0x19,
        Relogin = 0x1A,
        AccountLogin = 0x1B,


        Storage = 0x20,
        NpcResult = 0x21,
        NpcShop = 0x22,
        NpcConverse = 0x23,
        NpcAction = 0xBB,

        ChannelChange = 0x27,
        CashShopMigration = 0x28,
        TakeDamage = 0x2A,
        PlayerChat = 0x2C,
        UseChair = 0x2D,
        MagicAttack = 0x2E,
        ChangeMap = 0x2F,

        PlayerMovement = 0x35,
        RangedAttack = 0x36,
        PartyChat = 0x3A,
        GuildOperation = 0x3C,

        PartyOperation = 0x31,
        DenyPartyRequest = 0x32,
        BuddyListModify = 0x33,

        Messenger = 0x3D,
        PlayerInteraction = 0x3E,
        HiredMerchant = 0x3F,

        UseDoor = 0x41,
        PlayerInformation = 0x44,
        SpawnPet = 0x45,
        EnterPortal = 0x47,
        SkillEffect = 0x48,
        CancelItemEffect = 0x49,
        UseSummonBag = 0x4B,

        DistributeSP = 0x4D,
        CancelBuff = 0x4E,

        UseCashItem = 0x53,
        CloseRangeAttack = 0x59,
        FaceExpression = 0x5C,
        UseItemEffect = 0x5D,

        InventoryAction = 0x62,
        UseItem = 0x63,
        UseReturnScroll = 0x64,
        UseUpgradeScroll = 0x65,
        DistributeAP = 0x66,
        HealOverTime = 0x67,
        MesoDrop = 0x68,
        GiveFame = 0x69,
        QuestAction = 0x6B,

        ChangeKeymap = 0x75,
        MtsMigration = 0x77,
        DamageSummon = 0x79,
        SummonAttack = 0x7B,
        MoveSummon = 0x7C,

        MobMovement = 0x9D,

        PetChat = 0x82,
        PetCommand = 0x80,
        MovePet = 0x84,
        DropPickup = 0x89,
        DamageReactor = 0x8C,

        BbsOperation = 0x98,

        CashShopOperation = 0xAA,
        BuyCashItem = 0xAB,
        CouponCode = 0xAC,


        //ITEM_MOVE = 0x62
        //USE_ITEM = ITEM_MOVE 1
        //SPECIAL_MOVE = 0x51
        Whisper = 0x58,

        //CANCEL_CHAIR = 0x2B
        //NPC_ACTION = 0x98

        Sit = 42,
        CloseChalkboard = 50,
        UseDeathItem = 0xFF,
        AdminShopAction = 68,
        UsePetFood = 0x4C,
        UseMountFood = 0x4D,
        UseScriptedItem = 0x4E,
        UseCatchItem = 0x50,
        UseSkillBook = 0x51,
        UseTeleportRock = 84,
        UseSkill = 0x5B,
        CancelDebuff = 0x63,
        UseInnerPortal = 101,
        TrockAction = 102,
        Report = 106,

        SpouseChat = 0x6E,
        UseFishingItem = 0x6F,
        UseRemote = 0x73,
        MultiChat = 119,
        Command = 120,
        DenyGuildRequest = 127,
        AdminCommand = 128,
        AdminLog = 129,
        NoteAction = 131,
        MobAutomaticProvoke = 189,
        NpcMovement = 197,
        RingAction = 136,
        PetTalk = 0x9B,
        UseSolomonItem = 0x9C,
        PetLoot = 0xA4,
        PetAutoPot = 0xA5,
        PetExcludeItems = 0xA6,
        Beholder = 0xAC,
        MobDamageModFriendly = 0xB6,
        MonsterBomb = 0xB7,
        MobDamageMob = 0xB8,
        ChangedMap = 0xC4,
        MonsterCarnival = 0xD0,
        PlayerUpdate = 0xD5,
        LeaveField = 0xDF,
        HitReactor = 205,
        TouchReactor = 206,
    }
}