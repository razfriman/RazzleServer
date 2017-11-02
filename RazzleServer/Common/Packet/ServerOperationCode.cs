namespace RazzleServer.Common.Packet
{
    public enum ServerOperationCode : ushort
    {
        /*CLogin::OnPacket*/
        CheckPasswordResult = 0,
        AccountInfoResult = 2,
        CheckUserLimitResult = 3,
        SetAccountResult = 4,
        ConfirmEULAResult = 5,
        CheckPinCodeResult = 6,
        UpdatePinCodeResult = 7,
        ViewAllCharResult = 8,
        SelectCharacterByVACResult = 9,
        WorldInformation = 10,
        SelectWorldResult = 11,
        SelectCharacterResult = 12,
        CheckDuplicatedIDResult = 13,
        CreateNewCharacterResult = 14,
        DeleteCharacterResult = 15,
        /*CClientSocket::ProcessPacket*/
        MigrateCommand = 16,
        Ping = 17,
        AuthenCodeChanged = 18,
        AuthenMessage = 19,
        ChannelSelected = 20,
        HackshieldRequest = 21,
        WorldCharacterCreationDisabled = 22,
        MapleEuropeRequired = 23,
        //24
        CheckCRCResult = 25,
        LastConnectedWorld = 26,
        RecommendedWorldMessage = 27,
        CheckSPWResult = 28,

        /*CWvsContext::OnPacket*/
        InventoryOperation = 29,
        InventoryGrow = 30,
        StatChanged = 31,
        TemporaryStatSet = 32,
        TemporaryStatReset = 33,
        ForcedStatSet = 34,
        ForcedStatReset = 35,
        ChangeSkillRecordResult = 36,
        SkillUseResult = 37,
        GivePopularityResult = 38,
        Message = 39,
        OpenFullClientDownloadLink = 40,
        MemoResult = 41,
        MapTransferResult = 42,
        AntiMacroResult = 43,
        //44
        ClaimResult = 45,
        SetClaimSvrAvailableTime = 46,
        ClaimSvrStatusChanged = 47,
        SetTaminbMobInfo = 48,
        QuestClear = 49,
        EntrustedShopCheckResult = 50,
        SkillLearnItemResult = 51,
        GatherItemResult = 52,
        SortItemResult = 53,
        //54
        SueCharacterResult = 55,
        //56
        TradeMoneyLimit = 57,
        SetGender = 58,
        GuildBBSPacket = 59,
        //60
        CharacterInformation = 61,
        PartyResult = 62,
        FriendResult = 63,
        //64
        GuildResult = 65,
        AllianceResult = 66,
        TownPortal = 67,
        BroadcastMsg = 68,
        IncubatorResult = 69,
        ShopScannerResult = 70,
        ShopLinkResult = 71,
        MarriageRequest = 72,
        MarriageResult = 73,
        WeddingGiftResult = 74,
        NotifyMarriedPartnerMapTransfer = 75,
        CashPetFoodResult = 76,
        SetWeekEventMessage = 77,
        SetPotionDiscountRate = 78,
        BridleMobCatchFail = 79,
        ImitatedNPCResult = 80,
        NpcImitateData = 81,
        NpcUpdateLimitedDisableInfo = 82,
        HourChanged = 85,
        MiniMapOnOff = 86,
        ConsultAuthkeyUpdate = 87,
        ClassCompetitionAuthkeyUpdate = 88,
        WebBoardAuthkeyUpdate = 89,
        SessionValue = 90,
        PartyValue = 91,
        FieldSetVariable = 92,
        BonusExpRateChanged = 93,
        FamilyChartResult = 94,
        FamilyInfoResult = 95,
        FamilyResult = 96,
        FamilyJoinRequest = 97,
        FamilyJoinRequestResult = 98,
        FamilyJoinAccepted = 99,
        FamilyPrivilegeList = 100,
        FamilyFamousPointIncResult = 101,
        FamilyNotifyLoginOrLogout = 102,
        FamilySetPrivilege = 103,
        FamilySummonRequest = 104,
        NotifyLevelUp = 105,
        NotifyWedding = 106,
        NotifyJobChange = 107,
        //108
        MapleTVUseRes = 109,
        AvatarMegaphoneRes = 110,
        SetAvatarMegaphone = 111,
        ClearAvatarMegaphone = 112,
        CancelNameChangeResult = 113,
        CancelTransferWorldResult = 114,
        DestroyShopResult = 115,
        FakeGMNotice = 116,
        SuccessInUsegachaponBox = 117,
        NewYearCardRes = 118,
        RandomMorphRes = 119,
        CancelNameChangebyOther = 120,
        SetBuyEquipExt = 121,
        ScriptProgressMessage = 122,
        DataCRCCheckFailed = 123,
        MacroSysDataInit = 124,

        /*CStage::OnPacket*/
        SetField = 125,
        SetITC = 126,
        SetCashShop = 127,

        /*CMapLoadable::OnPacket*/
        SetBackEffect = 128,
        SetMapObjectVisible = 129,
        ClearBackEffect = 130,

        /*CField::OnPacket*/
        TransferFieldReqInogred = 131,
        TransferChannelReqIgnored = 132,
        FieldSpecificData = 133,
        GroupMessage = 134,
        Whisper = 135,
        CoupleMessage = 136,
        SummonItemInavailable = 137,
        FieldEffect = 138,
        FieldObstacleOnOff = 139,
        FieldObstacleOnOffStatus = 140,
        FieldObstacleReset = 141,
        BlowWeather = 142,
        PlayJukebox = 143,
        AdminResult = 144,
        Quiz = 145,
        Desc = 146,
        Clock = 147,
        ContiMove = 148,
        ContiState = 149,
        SetQuestClear = 150,
        SetQuestTime = 151,
        WarnMessage = 152,
        SetObjectState = 153,
        DestroyClock = 154,
        AriantArenaShowResult = 155,
        StalkResult = 156,
        PyramidGauge = 157,
        PyramidScore = 158,
        //159
        /*CUserPool::OnPacket*/
        UserEnterField = 160,
        UserLeaveField = 161,
        /*CUserPool::OnUserCommonPacket*/
        UserChat = 162,
        //163
        Chalkboard = 164,
        AnnounceBox = 165,
        ShowConsumeEffect = 166,
        ShowScrollEffect = 167,

        /*CUser::OnPetPacket*/
        PetEnterField = 168,
        //169
        PetMove = 170,
        PetAction = 171,
        PetNameChanged = 172,
        PetLoadExceptionList = 173,
        PetActionCommand = 174,
        /*CSummonedPool::OnPacket*/
        SummonedCreated = 175,
        SummonedRemoved = 176,
        SummonedMove = 177,
        SummonedAttack = 178,
        SummonedHit = 179,
        SummonedSkill = 180,
        /*CUser::OnDragonPacket*/
        DragonEnterField = 181,
        DragonMove = 182,
        //183
        //184

        /*CUserPool::OnUserRemotePacket*/
        Move = 185,
        CloseRangeAttack = 186,
        RangedAttack = 187,
        MagicAttack = 188,
        EnergyAttack = 189,
        SkillPrepare = 190,
        SkillCancel = 191,
        Hit = 192,
        Emotion = 193,
        SetActiveEffectItem = 194,
        ShowUpgradeTombEffect = 195,
        SetActiveRemoteChair = 196,
        AvatarModified = 197,
        RemoteEffect = 198,
        SetTemporaryStat = 199,
        ResetTemporaryStat = 200,
        RecieveHP = 201,
        GuildNameChanged = 202,
        GuildMarkChanged = 203,
        ThrowGrenade = 204,

        /*CUserLocal::OnPacket*/
        Sit = 205,
        Effect = 206,
        Teleport = 207,
        //208
        MesoGiveSucceeded = 209,
        MesoGiveFailed = 210,
        QuestResult = 211,
        NotifyHPDecByField = 212,
        SkillCooltimeSet = 213,
        BalloonMsg = 214,
        PlayEventSound = 215,
        PlayMinigameSound = 216,
        MakerResult = 2176,
        //218
        OpenCLassCompetetionPage = 219,
        OpenUI = 220,
        OpenUIWithOption = 221,
        SetStandAloneMode = 222,
        HireTutor = 223,
        TutorMsg = 224,
        IncComboResponse = 225,
        //226
        //227
        //228
        RadioSchedule = 229,
        OpenSkillGuide = 230,
        NoticeMsg = 231,
        //232
        //233
        Cooldown = 234,
        //235
        
        /*CMobPool::OnPacket*/
        MobEnterField = 236,
        MobLeaveField = 237,
        MobChangeController = 238,
        /*CMobPool::OnMobPacket*/
        MobMove = 239,
        MobCtrlAck = 240,
        //241
        MobStatSet = 242,
        MobStatReset = 243,
        MobSuspendReset = 244,
        MobAffected = 245,
        MobDamaged = 246,
        MobSpecialEffectBySkill = 247,
        //248
        MobCrcKeyChanged = 249,
        MobHPIndicator = 250,
        MobCatchEffect = 251,
        MobEffectByItem = 252,
        MobSpeaking = 253,
        MobIncMobChargeCount = 254,
        MobAttackedByMob = 255,
        //256

        /*CNpcPool::OnPacket*/
        NpcEnterField = 257,
        NpcLeaveField = 258,
        NpcChangeController = 259,
        /*CNpcPool::OnNpcPacket*/
        NpcMove = 260,
        NpcUpdateLimitedInfo = 261,
        NpcSetSpecialAction = 262,
        /*CNpcPool::OnNpcTemplatePacket*/
        NpcSetNpcScript = 263,

        //264

        /*CEmployeePool::OnPacket*/
        EmployeeEnterField = 265,
        EmployeeLeaveField = 266,
        EmployeeMiniRoomBalloon = 267,

        /*CDropPool::OnPacket*/
        DropEnterField = 268,
        DropLeaveField = 269,

        /*CMessageBoxPool::OnPacket*/
        MessageBoxCreateFailed = 270,
        MessageBoxEnterField = 271,
        MessageBoxLeaveField = 272,

        /*CAffectedAreaPool::OnPacket*/
        AffectedAreaCreated = 273,
        AffectedAreaRemoved = 274,

        /*CTownPortalPool::OnPacket*/
        TownPortalCreated = 275,
        TownPortalRemoved = 276,

        /*CReactorPool::OnPacket*/
        ReactorChangeState = 277,
        ReactorMove = 278, // NOTE: May not be implemented in v83 client.
        ReactorEnterField = 279,
        ReactorLeaveField = 280,

        SnowballState = 281,
        HitSnowball = 282,
        SnowballMejssage = 283,
        LeftKb = 284,
        CoconutHit = 285,
        CoconutScore = 286,
        GuildBossHealerMove = 287,
        GuildBossPulleyStateChange = 288,
        MonsterCarnivalEnter = 289,
        MonsterCarnivalPersonalCP = 290,
        MonsterCarnivalTeamCP = 291,
        MonsterCarnivalRequestResult1 = 292,
        MonsterCarnivalRequestResult0 = 293,
        MonsterCarnivalProcessForDeath = 294,
        MonsterCarnivalShowMemberOutMsg = 295,
        MonsterCarnivalShowGameResult = 296,
        AriantAreaUserScore = 297,
        //298
        SheepRanchInfo = 299,
        SheepRanchClothes = 300,
        AriantScore = 301,
        HorntailCave = 302,
        ZakumShrine = 303,

        /*CScriptMan::OnPacket*/
        ScriptMessage = 304,

        /*CShopDlg::OnPacket*/
        OpenNpcShop = 305,
        ConfirmShopTransaction = 306,

        /*CAdminShopDlg::OnPacket*/
        AdminShopMessage = 307,
        AdminShop = 308,
        Storage = 309,

        /*CStoreBankDlg::OnPacket*/
        StoreBankDlgMessage = 310, // TODO: Correct naming.
        StoreBankDlgSet = 311, // TODO: Correct naming.

        RPSGame = 312,
        Messenger = 313,
        PlayerInteraction = 314,
        Tournament = 315,
        TournamentMatchTable = 316,
        TournamentSetPrize = 317,
        TournamentUEW = 318,
        TournamentCharacters = 319,
        WeddingProgress = 320,
        WeddingCeremonyEnd = 321,
        Parcel = 322,
        ChangeParamResult = 323,
        QuestCashResult = 324,
        CashShopOperation = 325,
        // 326
        //327
        //328
        //329
        //330
        //331
        //332
        //333
        //334

        /*CFuncKeyMappedMan::OnPacket*/
        FuncKeyMappedInit = 335,
        FuncKeyMappedPetConsumeItemInit = 336,
        FuncKeyMappedPetConsumeMPItemInit = 337,

        //338
        //339
        //340

        /*CMapleTVMan::OnPacket*/
        MapleTVSetMessage = 341,
        MapleTVClearMessage = 342,
        MapleTVSendMessageResult = 343,

        //344
        //345
        //346
        MTSOperation2 = 347,
        MTSOperation = 348,
        //349
        //350
        //351
        //352
        //353
        ViciousHammer = 354,

        /// ORIGINAL
        LOGIN_RESPONSE = 0x00,
        SEND_LINK = 0x01,
        SERVERSTATUS = 0x03,
        GENDER_DONE = 0x04,
        TOS = 0x05,
        PIN_OPERATION = 0x06,
        PIN_ASSIGNED = 0x07,
        ALL_CHARLIST = 0x08,
        SERVERLIST = 0x0A,
        CHARLIST = 0x0B,
        SERVER_IP = 0x0C,
        PIC_ASSIGNED = 0x1E,
        CHAR_NAME_RESPONSE = 0x0D,
        ADD_NEW_CHAR_ENTRY = 0x0E,
        DELETE_CHAR_RESPONSE = 0x0F,

        CHANGE_CHANNEL = 0x10,
        PING = 0x11,
        CHANNEL_SELECTED = 0x14,
        RELOG_RESPONSE = 0xFFFF,
        MODIFY_INVENTORY_ITEM = 0x1D,
        ENABLE_RECOMMENDED = 0xFFFF,
        SEND_RECOMMENDED = 0x1B,
        INVENTORY_OPERATION = 0x1E,
        UPDATE_STATS = 0x1F,
        GIVE_BUFF = 0x20,
        REMOVE_BUFF = 0x21,
        UNKNOWN = 0x1F,

        UPDATE_SKILLS = 0x24,
        FAME_RESPONSE = 0x26,
        SHOW_STATUS_INFO = 0x27,
        SHOW_NOTES = 0x28,
        TROCK_LOCATIONS = 0x29,
        LIE_DETECTOR = 0x2A,
        REPORT_RESPONSE = 0x2B,
        ENABLE_REPORT = 0x2F,
        SEND_TITLE_BOX = 0x2F,

        UPDATE_MOUNT = 0x30,
        USE_SKILL_BOOK = 0x30,
        SHOW_QUEST_COMPLETION = 0x31,
        HIRED_MERCHANT_BOX = 0x32,
        FINISH_SORT = 0x34,
        FINISH_SORT2 = 0x35,
        REPORTREPLY = 0x35,
        MESO_LIMIT = 0x36,
        GENDER = 0x3A,
        BBS_OPERATION = 0x38,
        CHAR_INFO = 0x3D,
        PARTY_OPERATION = 0x3E,
        BUDDYLIST = 0x3F,

        GUILD_OPERATION = 0x41,
        ALLIANCE_OPERATION = 0x42,
        SPAWN_PORTAL = 0x41,
        SERVER_NOTICE = 0x44,
        FAMILY_ACTION = 0x45,
        YELLOW_TIP = 0x4D,
        PLAYER_NPC = 0x4E,

        MONSTERBOOK_ADD = 0x53,
        MONSTER_BOOK_CHANGE_COVER = 0x54,
        ENERGY = 0x55,
        SHOW_PEDIGREE = 0x57,
        OPEN_FAMILY = 0x58,
        FAMILY_MESSAGE = 0x59,
        FAMILY_INVITE = 0x5A,
        FAMILY_MESSAGE2 = 0x5B,
        FAMILY_SENIOR_MESSAGE = 0x5C,
        FAMILY_GAIN_REP = 0x5E,

        FAMILY_USE_REQUEST = 0x61,
        CREATE_CYGNUS = 0x62,
        LOAD_FAMILY = 0x64,
        BLANK_MESSAGE = 0x65,
        AVATAR_MEGA = 0x6F,
        NAME_CHANGE_MESSAGE = 0x69,
        UNKNOWN_MESSAGE = 0x6B,
        GM_POLICE = 0x6C,
        SILVER_BOX = 0x6D,
        UNKNOWN_MESSAGE2 = 0x6E,

        ENTER_MAP = 0x7D,
        MTS_OPEN = 0x73,
        ENTER_CASH_SHOP = 0x7F,
        RESET_SCREEN = 0x76,
        CS_BLOCKED = 0x78,
        BLOCK_PORTAL = 0x7F,

        BLOCK_PORTAL_SHOP = 0x80,
        SHOW_EQUIP_EFFECT = 0x85,
        FORCED_MAP_EQUIP = 0x85,
        MULTICHAT = 0x86,
        WHISPER = 0x87,
        SPOUSE_CHAT = 0x88,
        BOSS_ENV = 0x8A,
        MAP_EFFECT = 0x8F,

        GM_PACKET = 0x90,
        OX_QUIZ = 0x91,
        GMEVENT_INSTRUCTIONS = 0x92,
        CLOCK = 0x93,
        BOAT_EFFECT = 0x94,
        STOP_CLOCK = 0x95,
        ARIANT_SCOREBOARD = 0x96,
        QUICK_SLOT = 0x9F,
        SHOW_MAGNET = 0x9F,

        SPAWN_PLAYER = 0xA0,
        REMOVE_PLAYER = 0xA1,
        PLAYER_CHAT = 0xA2,
        CHALKBOARD = 0xA4,
        UPDATE_CHAR_BOX = 0x95,
        SHOW_SCROLL_EFFECT = 0xA7,
        SPAWN_PET = 0xA8,
        MOVE_PET = 0xAA,
        PET_CHAT = 0xAB,
        PET_NAMECHANGE = 0xAC,
        PET_SHOW = 0xAD,
        PET_COMMAND = 0xAE,
        SPAWN_SPECIAL_MAPOBJECT = 0xAF,

        REMOVE_SPECIAL_MAPOBJECT = 0xB0,
        SHOW_ITEM_EFFECT = 0xB0,
        MOVE_SUMMON = 0xB1,
        SUMMON_ATTACK = 0xB2,
        DAMAGE_SUMMON = 0xB3,
        SUMMON_SKILL = 0xB4,
        MOVE_PLAYER = 0xB9,
        CLOSE_RANGE_ATTACK = 0xBA,
        RANGED_ATTACK = 0xBB,
        MAGIC_ATTACK = 0xBC,
        SHOW_SKILL_EFFECT = 0xBE,
        CANCEL_SKILL_EFFECT = 0xBF,

        DAMAGE_PLAYER = 0xC0,
        FACIAL_EXPRESSION = 0xC1,
        SHOW_CHAIR = 0xC4,
        UPDATE_CHAR_LOOK = 0xC5,
        SHOW_FOREIGN_EFFECT = 0xC6,
        GIVE_FOREIGN_BUFF = 0xC7,
        CANCEL_FOREIGN_BUFF = 0xC8,
        UPDATE_PARTYMEMBER_HP = 0xC9,
        CANCEL_CHAIR = 0xCD,
        SHOW_ITEM_GAIN_INCHAT = 0xCE,
        DOJO_WARP_UP = 0xCF,

        LUCKSACK_PASS = 0xD0,
        LUCKSACK_FAIL = 0xD1,
        MESO_BAG_MESSAGE = 0xD2,
        UPDATE_QUEST_INFO = 0xD3,
        PLAYER_HINT = 0xC4,
        KOREAN_EVENT = 0xC9,
        CYGNUS_CHAR_CREATED = 0xCC,

        DAMAGE_MONSTER = 0xDA,
        ARIANT_THING = 0xDD,
        CYGNUS_INTRO_LOCK = 0xDD,
        CYGNUS_INTRO_DISABLE_UI = 0xDE,
        SHOW_DRAGGED = 0xDF,

        GIVE_COOLDOWN = 0xEA,
        SPAWN_MONSTER = 0xEC,
        KILL_MONSTER = 0xED,
        SPAWN_MONSTER_CONTROL = 0xEE,
        MOVE_MONSTER = 0xEF,

        KITE_MESSAGE = 0xF0,
        KITE = 0xF1,
        MOVE_MONSTER_RESPONSE = 0xF0,
        APPLY_MONSTER_STATUS = 0xF2,
        CANCEL_MONSTER_STATUS = 0xF3,
        SHOW_MONSTER_HP = 0xFA,
        ROLL_SNOWBALL = 0xFB,
        HIT_SNOWBALL = 0xFC,
        SNOWBALL_MESSAGE = 0xFD,
        LEFT_KNOCK_BACK = 0xFE,

        UNABLE_TO_CONNECT = 0x100,
        CATCH_MONSTER = 0x100,
        SPAWN_NPC = 0x101,
        REMOVE_NPC = 0x102,
        SPAWN_NPC_REQUEST_CONTROLLER = 0x103,
        NPC_ACTION = 0x104,
        SPAWN_HIRED_MERCHANT = 0x109,
        DESTROY_HIRED_MERCHANT = 0x10A,
        UPDATE_HIRED_MERCHANT = 0x10B,
        DROP_ITEM_FROM_MAPOBJECT = 0x10C,
        REMOVE_ITEM_FROM_MAP = 0x10D,
        MONSTER_CARNIVAL_START = 0x103,
        MONSTER_CARNIVAL_OBTAINED_CP = 0x104,
        MONSTER_CARNIVAL_PARTY_CP = 0x105,
        MONSTER_CARNIVAL_SUMMON = 0x106,
        MONSTER_CARNIVAL_DIED = 0x108,
        ARIANT_PQ_START = 0x10B,
        ZAKUM_SHRINE = 0x10D,

        SPAWN_MIST = 0x111,
        REMOVE_MIST = 0x112,
        SPAWN_DOOR = 0x113,
        REMOVE_DOOR = 0x114,
        REACTOR_HIT = 0x115,
        REACTOR_SPAWN = 0x117,
        REACTOR_DESTROY = 0x118,

        DUEY = 0x120,
        AUTO_HP_POT = 0x12B,
        AUTO_MP_POT = 0x12C,

        NPC_TALK = 0x130,
        OPEN_NPC_SHOP = 0x131,
        CONFIRM_SHOP_TRANSACTION = 0x132,
        OPEN_STORAGE = 0x135,
        MTS_OPERATION2 = 0x136,
        MTS_OPERATION = 0x137,
        VICIOUS_HAMMER = 0x13D,
        MESSENGER = 0x139,
        PLAYER_INTERACTION = 0x13A,

        CS_UPDATE = 0x144,
        CS_OPERATION = 0x145,
        KEYMAP = 0x14F,

        SEND_TV = 0x155,
        REMOVE_TV = 0x156,
        ENABLE_TV = 0x157,

        CATCH_ARIANT = 0xFF,
        CATCH_MOUNT = 0xFF,
        HPQ_MOON = 0xFF,
        ARIANT_SCORE = 0xFF,
        GIVE_MONSTER_BUFF = 0xFF,
        SPAWN_MYSTIC_DOOR = 0xFF,
    }
}
