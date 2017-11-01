namespace RazzleServer.Common.Packet
{
    public enum ClientOperationCode : ushort
    {
        AccountLogin = 1,
        GuestLogin = 2,
        AccountInfo = 3,
        WorldRelist = 4,
        WorldSelect = 5,
        WorldStatus = 6,
        AccountGender = 8,
        PinCheck = 9,
        PinUpdate = 10,
        WorldList = 11,
        LeaveCharacterSelect = 12,
        ViewAllChar = 13,
        SelectCharacterByVAC = 14,
        VACFlagSet = 15,
        //16
        //17
        //18
        CharacterSelect = 19,
        CharacterLoad = 20,
        CharacterNameCheck = 21,
        CharacterCreate = 22,
        CharacterDelete = 23,
        //24
        ClientError = 25,
        Pong = 26,
        //27
        //28
        CharacterSelectRegisterPic = 29,
        CharacterSelectRequestPic = 30,
        RegisterPicFromVAC = 31,
        RequestPicFromVAC = 32,
        //32
        //33
        //34
        ClientStart = 35,
        //36
        //37
        MapChange = 38,
        ChannelChange = 39,
        CashShopMigration = 40,
        PlayerMovement = 41,
        Sit = 42,
        UseChair = 43,
        CloseRangeAttack = 44,
        RangedAttack = 45,
        MagicAttack = 46,
        EnergyOrbAttack = 47,
        TakeDamage = 48,
        PlayerChat = 49,
        CloseChalkboard = 50,
        FaceExpression = 51,
        UseItemEffect = 52,
        UseDeathItem = 53,
        //54
        //55
        //56
        //MonsterBookCover? = 57
        NpcConverse = 58,
        //RemoteStore? = 59
        NpcResult = 60,
        NpcShop = 61,
        Storage = 62,
        HiredMerchant = 0x3F,
        //DueyAction? FredrickAction? = 0x40,
        //65
        //66
        //67
        AdminShopAction = 68,
        InventorySort = 0x45,
        InventoryGather = 0x46,
        InventoryAction = 0x47,
        UseItem = 0x48,
        CancelItemEffect = 0x49,
        UseSummonBag = 0x4B,
        UsePetFood = 0x4C,
        UseMountFood = 0x4D,
        UseScriptedItem = 0x4E,
        UseCashItem = 0x4F,

        #region QUESTIONABLE
        UseCatchItem = 0x50,
        UseSkillBook = 0x51,
        //82
        //89
        #endregion QUESTIONABLE

        UseTeleportRock = 84,
        UseReturnScroll = 85,
        UseUpgradeScroll = 86,
        DistributeAP = 87,
        AutoDistributeAP = 88,
        HealOverTime = 89,
        DistributeSP = 0x5A,
        UseSkill = 0x5B,
        CancelBuff = 0x5C,
        SkillEffect = 0x5D,
        MesoDrop = 0x5E,
        GiveFame = 0x5F,
        PlayerInformation = 97,
        SpawnPet = 0x62,
        CancelDebuff = 0x63,
        ChangeMapSpecial = 100,
        UseInnerPortal = 101,
        TrockAction = 102,
        Report = 106,

        #region QUESTIONABLE
        //103
        //104
        //106
        QuestAction = 107,
        //108
        SkillMacro = 0x6D,
        SpouseChat = 0x6E,
        UseFishingItem = 0x6F,
        MakerSkill = 0x70,
        //113
        //114
        UseRemote = 0x73,
        PartyChat = 0x74,
        //115
        //117
        //119
        //120
        //121
        //122
        #endregion QUESTIONABLE

        MultiChat = 119,
        Command = 120,
        Messenger = 122,
        PlayerInteraction = 123,
        PartyOperation = 124,
        DenyPartyRequest = 125,
        GuildOperation = 126,
        DenyGuildRequest = 127,
        AdminCommand = 128,
        AdminLog = 129,
        BuddyListModify = 130,
        NoteAction = 131,
        UseDoor = 132,
        ChangeKeymap = 135,
        FamilyPedigree = 145,
        FamilyOpen = 146,
        BbsOperation = 155,
        MovePet = 167,
        MobAutomaticProvoke = 189,
        NpcMovement = 197,

        #region QUESTIONABLE
        RingAction = 136,
        OpenFamily = 0x90,
        AddFamily = 0x91,
        AcceptFamily = 0x94,
        UseFamily = 0x95,
        AllianceOperation = 0x96,
        MtsMigration = 0x9A,
        PetTalk = 0x9B,
        UseSolomonItem = 0x9C,
        PetChat = 0xA2,
        PetCommand = 0xA3,
        PetLoot = 0xA4,
        PetAutoPot = 0xA5,
        PetExcludeItems = 0xA6,
        MoveSummon = 0xA9,
        SummonAttack = 0xAA,
        DamageSummon = 0xAB,
        Beholder = 0xAC,
        MobMovement = 0xBC,
        MobDamageModFriendly = 0xB6,
        MonsterBomb = 0xB7,
        MobDamageMob = 0xB8,
        NpcAction = 0xBB,
        DropPickup = 0xCA,
        DamageReactor = 0xC3,
        ChangedMap = 0xC4,
        MonsterCarnival = 0xD0,
        PlayerUpdate = 0xD5,
        CashShopOperation = 0xDA,
        BuyCashItem = 0xDB,
        CouponCode = 0xDC,
        LeaveField = 0xDF,
        OpenItemInterface = 0xE1,
        CloseItemInterface = 0xE2,
        UseItemInterface = 0xE3,
        MtsOperation = 0xF1,
        UseMapleLife = 0xF4,
        UseHammer = 0xF8,
        MapleTV = 0x222,
        #endregion QUESTIONABLE

        HitReactor = 205,
        TouchReactor = 206,
        //207
        PartySearchStart = 222,
        PartySearchStop = 223,


        //// ORIGINAL
        LOGIN_REQUEST = 0x01,
        GUEST_LOGIN_REQUEST = 0x02,
        SERVERLIST_REREQUEST = 0x04,
        CHARLIST_REQUEST = 0x05,
        SERVERSTATUS_REQUEST = 0x06,
        SET_GENDER = 0x08,
        AFTER_LOGIN = 0x09,
        REGISTER_PIN = 0x0A,
        SERVERLIST_REQUEST = 0x0B,
        PLAYER_DC = 0xC0,
        VIEW_ALL_CHAR = 0x0D,
        PICK_ALL_CHAR = 0x0E,

        PLAYER_LOGGEDIN = 0x14,
        CHECK_CHAR_NAME = 0x15,
        CREATE_CHAR = 0x16,
        DELETE_CHAR = 0x17,
        PONG = 0x18,
        CLIENT_START = 0x19,
        CLIENT_ERROR = 0x1B,
        STRANGE_DATA = 0x1C,
        RELOG = 0x1D,
        CHAR_SELECT = 0x1E,

        CREATE_CYGNUS = 0xFF,

        UNKNOWN = 0x23,
        CHANGE_MAP = 0x26,
        CHANGE_CHANNEL = 0x27,
        ENTER_CASH_SHOP = 0x28,
        MOVE_PLAYER = 0x29,
        CANCEL_CHAIR = 0x2A,
        USE_CHAIR = 0x2B,
        CLOSE_RANGE_ATTACK = 0x2C,
        RANGED_ATTACK = 0x2D,
        MAGIC_ATTACK = 0x2E,
        ENERGY_ORB_ATTACK = 0x2F,
        TAKE_DAMAGE = 0x30,

        GENERAL_CHAT = 0x31,
        CLOSE_CHALKBOARD = 0x32,
        FACE_EXPRESSION = 0x33,
        USE_ITEMEFFECT = 0x34,
        USE_DEATHITEM = 0x35,
        MONSTER_BOOK_COVER = 0x39,
        NPC_TALK = 0x3A,
        NPC_TALK_MORE = 0x3C,
        NPC_SHOP = 0x3D,
        STORAGE = 0x3E,
        HIRED_MERCHANT_REQUEST = 0x3F,

        DUEY_ACTION = 0x40,
        ITEM_SORT = 0x44,
        ITEM_SORT2 = 0x45,
        ITEM_MOVE = 0x47,
        USE_ITEM = 0x48,
        CANCEL_ITEM_EFFECT = 0x48FF,
        USE_SUMMON_BAG = 0x4AFF,
        PET_FOOD = 0x4C,
        USE_MOUNT_FOOD = 0x4D,
        SCRIPTED_ITEM = 0x4E,
        USE_CASH_ITEM = 0x4F,

        USE_CATCH_ITEM = 0x52,
        USE_SKILL_BOOK = 0x53,
        USE_TELEPORT_ROCK = 0x54,
        USE_RETURN_SCROLL = 0x55,
        USE_UPGRADE_SCROLL = 0x56,
        DISTRIBUTE_AP = 0x57,
        AUTO_DISTRIBUTE_AP = 0x58,
        HEAL_OVER_TIME = 0x59,
        DISTRIBUTE_SP = 0x5A,
        SPECIAL_MOVE = 0x5B,
        CANCEL_BUFF = 0x5C,
        SKILL_EFFECT = 0xC6,
        MESO_DROP = 0x5E,
        GIVE_FAME = 0x5F,

        CHAR_INFO_REQUEST = 0x61,
        SPAWN_PET = 0x62,
        CANCEL_DEBUFF = 0x63,
        CHANGE_MAP_SPECIAL = 0x64,
        USE_INNER_PORTAL = 0x65,
        REPORT = 0x69FF,
        TROCK_ADD_MAP = 0x65FF,
        QUEST_ACTION = 0x6B,
        SKILL_MACRO = 0x6E,
        SPOUSE_CHAT = 0x6EFF,
        USE_FISHING_ITEM = 0x6FFF,

        MAKER_SKILL = 0x70,
        USE_REMOTE = 0x73,
        PARTYCHAT = 0x77,
        WHISPER = 0x78,
        MESSENGER = 0x7A,
        PLAYER_INTERACTION = 0x7B,
        PARTY_OPERATION = 0x7C,
        DENY_PARTY_REQUEST = 0x7D,
        GUILD_OPERATION = 0x7E,
        DENY_GUILD_REQUEST = 0x7F,
        ADMIN_COMMAND = 0x80,
        ADMIN_LOG = 0x81,

        BUDDYLIST_MODIFY = 0x82,
        NOTE_ACTION = 0x81,
        USE_DOOR = 0x5BFF,
        RING_ACTION = 0x87FF,

        ADD_FAMILY = 0x91,
        OPEN_FAMILY = 0x92,
        ACCEPT_FAMILY = 0x94FF,
        USE_FAMILY = 0x95FF,
        ALLIANCE_OPERATION = 0x96FF,
        BBS_OPERATION = 0x99FF,
        ENTER_MTS = 0x9C,
        PET_TALK = 0x9BFF,
        USE_SOLOMON_ITEM = 0x9CFF,

        MOVE_PET = 0xA7,
        PET_CHAT = 0xA8,
        PET_COMMAND = 0xA9,
        PET_LOOT = 0xAA,
        PET_AUTO_POT = 0xAB,
        PET_EXCLUDE_ITEMS = 0xAC,
        MOVE_SUMMON = 0xAF,

        SUMMON_ATTACK = 0xB0,
        DAMAGE_SUMMON = 0xB1,
        CHANGE_KEYMAP = 0xB7,

        BEHOLDER = 0xACFF,

        MOVE_LIFE = 0xBC,
        AUTO_AGGRO = 0xBD,
        MOB_DAMAGE_MOB_FRIENDLY = 0xBE,
        MONSTER_BOMB = 0xBF,
        MOB_DAMAGE_MOB = 0xC0,
        NPC_ACTION = 0xC5,

        ITEM_PICKUP = 0xCA,
        DAMAGE_REACTOR = 0xCD,
        TOUCHING_REACTOR = 0xC4FF,

        MONSTER_CARNIVAL = 0xD0FF,
        PARTY_SEARCH_REGISTER = 0xD2FF,
        PARTY_SEARCH_START = 0xD4FF,
        MAPLETV = 0x22FF,

        PLAYER_UPDATE = 0xD5FF,
        TOUCHING_CS = 0xE4,
        BUY_CS_ITEM = 0xE5,
        COUPON_CODE = 0xE6,

        OPEN_ITEMUI = 0xE1FF,
        CLOSE_ITEMUI = 0xE2FF,
        USE_ITEMUI = 0xE3FF,

        MTS_OP = 0xF1FF,
        USE_MAPLELIFE = 0xF4FF,
        USE_HAMMER = 0xF8FF,
    }
}
