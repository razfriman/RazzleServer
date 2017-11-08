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
        PinCodeOperation = 0x0D,

        MigrateCommand = 0x10,
        CheckUserLimitResult = 0x12,
        SelectWorldResult = 0x13,
        ReloginResponse = 0x15,

        InventoryOperation = 0x18,
        AvatarMegaphoneRes = 0x19,
        QuestResult = 0x1F,

        StatChanged = 0x23,
        BuffCancel = 0x24,
        PortalSpawn = 0x29,
        ServerMessage = 0x2D,
        SkillsUpdate = 0x2F,

        FameResponse = 0x31,
        ShowStatusInfo = 0x32,
        ShowMesoGain = 0x33,
        BuffGive = 0x3B,

        SetField = 0x4E,

        //BOSS_ENV = 0x54
        //MULTICHAT = 0x56
        ShowApple = 0x5C,
        Whisper = 0x5F,

        Clock = 0x62,
        ShowPlayer = 0x66,
        CancelChair = 0x67,
        //SHOW_ITEM_GAIN_INCHAT = 0x68
        //UPDATE_QUEST_INFO = 0x6d

        //NoticeMsg = 231,

        Cooldown = 0x70,

        UserEnterField = 160,
        UserLeaveField = 161,

        //REMOVE_PLAYER_FROM_MAP = 0x71
        UserChat = 0x72,
        //SPAWN_SPECIAL_MAPOBJECT = 0x73
        //REMOVE_SPECIAL_MAPOBJECT = 0x74
        //MOVE_SUMMON = 0x75
        //SUMMON_ATTACK = 0x76
        //DAMAGE_SUMMON = 0x78
        //SHOW_SCROLL_EFFECT = 0x7B

        Move = 0x85,
        ForeignEffectShow = 0x86,
        CloseRangeAttack = 0x88,
        Hit = 0x8A,
        ForeignBuffCancel = 0x8B,
        PartyUpdateHp = 0x8C,
        Emotion = 0x8D,
        RangedAttack = 0x8E,
        ItemEffect = 0x8F,

        ShowChair = 0x92,
        AvatarModified = 0x93,
        MagicAttack = 0x94,
        MobSpawn = 0x97,
        MobMove = 0x98,
        MobApplyStatus = 0x9B,
        MobMoveResponse = 0x9D,




        MonsterControlSpawn = 0xA5,
        NpcSpawn = 0xA8,

        ReactorSpawn = 0xB3,
        //DROP_ITEM_FROM_MAPOBJECT = 0xB9
        MistSpawn = 0xBE,
        MistRemove = 0xBF,

        DoorSpawn = 0xC0,
        DoorRemove = 0xC1,

        Keymap = 0xF7,

        ForcedStatSet = 34,
        ForcedStatReset = 35,
        ChangeSkillRecordResult = 36,
        Message = 39,
        MemoResult = 41,
        MapTransferResult = 42,
        QuestClear = 49,
        SueCharacterResult = 55,
        SetGender = 58,
        CharacterInformation = 61,
        BroadcastMsg = 68,
        SetAvatarMegaphone = 111,
        TransferFieldReqInogred = 131,
        GroupMessage = 134,
        AdminResult = 144,
        Chalkboard = 164,
        AnnounceBox = 165,
        RemoteEffect = 198,
        SetTemporaryStat = 199,
        ResetTemporaryStat = 200,
        Sit = 205,
        Effect = 206,
        Teleport = 207,
        BalloonMsg = 214,
        MobEnterField = 236,
        MobLeaveField = 237,
        MobChangeController = 238,
        MobCtrlAck = 240,
        MobStatSet = 242,
        MobStatReset = 243,
        MobDamaged = 246,
        MobHPIndicator = 250,
        NpcEnterField = 257,
        NpcLeaveField = 258,
        NpcChangeController = 259,
        NpcMove = 260,
        DropEnterField = 268,
        DropLeaveField = 269,
        ReactorChangeState = 277,
        ReactorEnterField = 279,
        ReactorLeaveField = 280,
        ScriptMessage = 304,
        OpenNpcShop = 305,
        ConfirmShopTransaction = 306,
        AdminShopMessage = 307,
        AdminShop = 308,
        Storage = 309,
        PlayerInteraction = 314,
        TemporaryStatSet = 333,
        TemporaryStatReset = 333,
    }
}
