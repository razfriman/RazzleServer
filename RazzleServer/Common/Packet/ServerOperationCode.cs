﻿/*# This file is part of the OdinMS Maple Story Server


# CHANNEL

CHAR_INFO = SPAWN_PORTAL 1
BUDDYLIST = CHAR_INFO 1
PARTY_OPERATION = 0x39
CANCEL_CHAIR = SPAWN_PLAYER 1
GIVE_FOREIGN_BUFF = SHOW_FOREIGN_EFFECT 1

SHOW_MONSTER_HP = MOVE_MONSTER 1
CANCEL_MONSTER_STATUS = APPLY_MONSTER_STATUS 1
DAMAGE_MONSTER = MOVE_MONSTER_RESPONSE 1
KILL_MONSTER = SPAWN_MONSTER_CONTROL 1
SPAWN_NPC_REQUEST_CONTROLLER = SPAWN_NPC 5
REACTOR_HIT = REACTOR_SPAWN 1
REACTOR_DESTROY = REACTOR_HIT 1
REMOVE_ITEM_FROM_MAP = DROP_ITEM_FROM_MAPOBJECT 1
NPC_TALK = 0xC3
OPEN_NPC_SHOP = 0xD7
CONFIRM_SHOP_TRANSACTION = OPEN_NPC_SHOP 1
OPEN_STORAGE = CONFIRM_SHOP_TRANSACTION 1
PLAYER_INTERACTION = 0xDE
KEYMAP = 0xF7
MAP_EFFECT = 0x55
NPC_ACTION = 0xAE
UPDATE_CHAR_BOX = 0xFFFF
GUILD_OPERATION = 0x28
BBS_OPERATION = 0x42
SKILL_EFFECT = 0x91
CANCEL_SKILL_EFFECT = 0x89
SUMMON_SKILL = 0x77
SHOW_MAGNET = 0x9A
MESSENGER = 0xDD
SPAWN_PET = 0x7D
MOVE_PET = 0x82
PET_CHAT = 0x7E
PET_COMMAND = 0x83
PET_NAMECHANGE = 0x80
*/

namespace RazzleServer.Common.Packet
{
    public enum ServerOperationCode : ushort
    {
        CheckPasswordResult = 0x00,
        AccountInfoResult = 0x02,
        ChannelSelected = 0x03,
        SetAccountResult = 0x04,
        WorldInformation = 0x05,
        CheckCharacterNameResult = 0x06,
        CreateNewCharacterResult = 0x07,
        DeleteCharacterResult = 0x08,
        ViewAllCharResult = 0x08,
        SelectCharacterByVACResult = 0x09,
        Ping = 0x09,
        SelectCharacterResult = 0x0C,
        CheckPinCodeResult = 0x0D,
        UpdatePinCodeResult = 0x0D,

        MigrateCommand = 0x10,
        CheckUserLimitResult = 0x12,
        SelectWorldResult = 0x13,
        ReloginResponse = 0x15,

        //MODIFY_INVENTORY_ITEM = 0x18
        AvatarMegaphoneRes = 0x19,
        //SHOW_QUEST_COMPLETION = 0x1F

        //UPDATE_STATS = 0x23
        //CANCEL_BUFF = 0x24
        //SPAWN_PORTAL = 0x29
        //SERVERMESSAGE = 0x2D
        //UPDATE_SKILLS = 0x2F

        //FAME_RESPONSE = 0x31
        //SHOW_STATUS_INFO = 0x32
        //SHOW_MESO_GAIN = 0x33
        //GIVE_BUFF = 0x3B

        SetField = 0x4E,

        //BOSS_ENV = 0x54
        //MULTICHAT = 0x56
        ShowApple = 0x5C,
        Whisper = 0x5F,

        //CLOCK = 0x62
        //SPAWN_PLAYER = 0x66
        //SHOW_ITEM_GAIN_INCHAT = 0x68
        //UPDATE_QUEST_INFO = 0x6d

        //COOLDOWN = 0x70
        //REMOVE_PLAYER_FROM_MAP = 0x71
        //CHATTEXT = 0x72
        //SPAWN_SPECIAL_MAPOBJECT = 0x73
        //REMOVE_SPECIAL_MAPOBJECT = 0x74
        //MOVE_SUMMON = 0x75
        //SUMMON_ATTACK = 0x76
        //DAMAGE_SUMMON = 0x78
        //SHOW_SCROLL_EFFECT = 0x7B

        Move = 0x85,
        //SHOW_FOREIGN_EFFECT = 0x86
        CloseRangeAttack = 0x88,
        Hit = 0x8A,
        //CANCEL_FOREIGN_BUFF = 0x8B
        //UPDATE_PARTYMEMBER_HP = 0x8C
        Emotion = 0x8D,
        RangedAttack = 0x8E,
        //SHOW_ITEM_EFFECT = 0x8F

        //SHOW_CHAIR = 0x92
        //UPDATE_CHAR_LOOK = 0x93
        MagicAttack = 0x94,
        //SPAWN_MONSTER = 0x97
        //MOVE_MONSTER = 0x98
        //APPLY_MONSTER_STATUS = 0x9B
        //MOVE_MONSTER_RESPONSE = 0x9D

        //SPAWN_MONSTER_CONTROL = 0xA5
        //SPAWN_NPC = 0xA8

        //REACTOR_SPAWN = 0xB3
        //DROP_ITEM_FROM_MAPOBJECT = 0xB9
        //SPAWN_MIST = 0xBE
        //REMOVE_MIST = 0xBF

        //SPAWN_DOOR = 0xC0
        //REMOVE_DOOR = 0xC1

        Keymap = 0xF7,

        EnergyAttack = 189,
        SkillPrepare = 190,
        SkillCancel = 191,
        WorldCharacterCreationDisabled = 22,
        LastConnectedWorld = 26,
        RecommendedWorldMessage = 27,
        CheckSPWResult = 28,
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
        ClaimResult = 45,
        SetClaimSvrAvailableTime = 46,
        SetTaminbMobInfo = 48,
        QuestClear = 49,
        EntrustedShopCheckResult = 50,
        SkillLearnItemResult = 51,
        GatherItemResult = 52,
        SortItemResult = 53,
        SueCharacterResult = 55,
        TradeMoneyLimit = 57,
        SetGender = 58,
        GuildBBSPacket = 59,
        CharacterInformation = 61,
        PartyResult = 62,
        FriendResult = 63,
        GuildResult = 65,
        TownPortal = 67,
        BroadcastMsg = 68,
        IncubatorResult = 69,
        ShopScannerResult = 70,
        ShopLinkResult = 71,
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
        NotifyLevelUp = 105,
        NotifyJobChange = 107,
        MapleTVUseRes = 109,
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
        SetITC = 126,
        SetCashShop = 127,
        SetBackEffect = 128,
        SetMapObjectVisible = 129,
        ClearBackEffect = 130,
        TransferFieldReqInogred = 131,
        TransferChannelReqIgnored = 132,
        FieldSpecificData = 133,
        GroupMessage = 134,
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
        StalkResult = 156,
        PyramidGauge = 157,
        PyramidScore = 158,
        UserEnterField = 160,
        UserLeaveField = 161,
        UserChat = 162,
        Chalkboard = 164,
        AnnounceBox = 165,
        ShowConsumeEffect = 166,
        ShowScrollEffect = 167,
        PetEnterField = 168,
        PetMove = 170,
        PetAction = 171,
        PetNameChanged = 172,
        PetLoadExceptionList = 173,
        PetActionCommand = 174,
        SummonedCreated = 175,
        SummonedRemoved = 176,
        SummonedMove = 177,
        SummonedAttack = 178,
        SummonedHit = 179,
        SummonedSkill = 180,
        DragonEnterField = 181,
        DragonMove = 182,
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
        Sit = 205,
        Effect = 206,
        Teleport = 207,
        MesoGiveSucceeded = 209,
        MesoGiveFailed = 210,
        QuestResult = 211,
        NotifyHPDecByField = 212,
        SkillCooltimeSet = 213,
        BalloonMsg = 214,
        PlayEventSound = 215,
        PlayMinigameSound = 216,
        OpenCLassCompetetionPage = 219,
        OpenUI = 220,
        OpenUIWithOption = 221,
        SetStandAloneMode = 222,
        HireTutor = 223,
        TutorMsg = 224,
        IncComboResponse = 225,
        RadioSchedule = 229,
        OpenSkillGuide = 230,
        NoticeMsg = 231,
        Cooldown = 234,
        MobEnterField = 236,
        MobLeaveField = 237,
        MobChangeController = 238,
        MobMove = 239,
        MobCtrlAck = 240,
        MobStatSet = 242,
        MobStatReset = 243,
        MobSuspendReset = 244,
        MobAffected = 245,
        MobDamaged = 246,
        MobSpecialEffectBySkill = 247,
        MobCrcKeyChanged = 249,
        MobHPIndicator = 250,
        MobCatchEffect = 251,
        MobEffectByItem = 252,
        MobSpeaking = 253,
        MobIncMobChargeCount = 254,
        MobAttackedByMob = 255,
        NpcEnterField = 257,
        NpcLeaveField = 258,
        NpcChangeController = 259,
        NpcMove = 260,
        NpcUpdateLimitedInfo = 261,
        NpcSetSpecialAction = 262,
        NpcSetNpcScript = 263,
        EmployeeEnterField = 265,
        EmployeeLeaveField = 266,
        EmployeeMiniRoomBalloon = 267,
        DropEnterField = 268,
        DropLeaveField = 269,
        MessageBoxCreateFailed = 270,
        MessageBoxEnterField = 271,
        MessageBoxLeaveField = 272,
        AffectedAreaCreated = 273,
        AffectedAreaRemoved = 274,
        TownPortalCreated = 275,
        TownPortalRemoved = 276,
        ReactorChangeState = 277,
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
        SheepRanchInfo = 299,
        SheepRanchClothes = 300,
        HorntailCave = 302,
        ZakumShrine = 303,
        ScriptMessage = 304,
        OpenNpcShop = 305,
        ConfirmShopTransaction = 306,
        AdminShopMessage = 307,
        AdminShop = 308,
        Storage = 309,
        StoreBankDlgMessage = 310,
        StoreBankDlgSet = 311,
        RPSGame = 312,
        Messenger = 313,
        PlayerInteraction = 314,
        Tournament = 315,
        TournamentMatchTable = 316,
        TournamentSetPrize = 317,
        TournamentUEW = 318,
        TournamentCharacters = 319,
        Parcel = 322,
        ChangeParamResult = 323,
        QuestCashResult = 324,
        CashShopOperation = 325,

        MapleTVSetMessage = 341,
        MapleTVClearMessage = 342,
        MapleTVSendMessageResult = 343,
        MTSOperation2 = 347,
        MTSOperation = 348,
    }
}
