using System;

namespace RazzleServer.Common.Constants
{
    #region Character

    public enum ReportType : byte
    {
        IllegalProgramUsage = 0,
        ConversationClaim = 1
    }

    public enum ReportResult : byte
    {
        Success,
        UnableToLocate,
        Max10TimesADay,
        YouAreReportedByUser,
        UnknownError
    }
    #endregion

    #region Equipment

    public enum EquippedQueryMode
    {
        Any,
        Cash,
        Normal
    }

    public enum WeaponType
    {
        NotAWeapon,
        Bow,
        Claw,
        Dagger,
        Crossbow,
        Axe1H,
        Sword1H,
        Blunt1H,
        Axe2H,
        Sword2H,
        Blunt2H,
        PoleArm,
        Spear,
        Staff,
        Wand,
        Knuckle
    }
    #endregion

    #region Items
    [Flags]
    public enum ItemFlags : short
    {
        Sealed = 0x01,
        AddPreventSlipping = 0x02,
        AddPreventColdness = 0x04,
        Untradeable = 0x08
    }

    public enum ItemType : byte
    {
        Equipment = 1,
        Usable = 2,
        Setup = 3,
        Etcetera = 4,
        Cash = 5,
        Count = 6
    }

    public enum InventoryOperationType : byte
    {
        AddItem,
        ModifyQuantity,
        ModifySlot,
        RemoveItem
    }

    public enum MemoAction : byte
    {
        Send = 0,
        Delete = 1
    }

    public enum MemoResult : byte
    {
        Send = 3,
        Sent = 4,
        Error = 5
    }

    public enum MemoError : byte
    {
        ReceiverOnline,
        ReceiverInvalidName,
        ReceiverInboxFull
    }

    public enum TrockAction : byte
    {
        Remove = 0,
        Add = 1
    }

    public enum TrockType : byte
    {
        Regular = 0,
        Vip = 1
    }

    public enum TrockResult : byte
    {
        Success,
        Unknown = 2,
        Unknown2 = 3,
        CannotGo2 = 5,
        DifficultToLocate = 6,
        DifficultToLocate2 = 7,
        CannotGo = 8,
        AlreadyThere = 9,
        CannotSaveMap = 10,
        NoobsCannotLeaveMapleIsland = 11
    }
    #endregion

    #region Interaction
    public enum InteractionCode : byte
    {
        Create = 0,
        Invite = 2,
        Decline = 3,
        Visit = 4,
        Room = 5,
        Chat = 6,
        Exit = 10,
        Open = 11,
        TradeBirthday = 14,
        SetItems = 15,
        SetMeso = 16,
        Confirm = 17,
        AddItem = 22,
        Buy = 23,
        UpdateItems = 25,
        RemoveItem = 27,
        OpenStore = 30
    }

    public enum InteractionType : byte
    {
        Omok = 1,
        Trade = 3,
        PlayerShop = 4,
        HiredMerchant = 5
    }
    #endregion

    #region Login
    public enum CharacterDeletionResult : byte
    {
        Valid = 0
    }

    public enum LoginResult
    {
        Valid = 0,
        Banned = 3,
        InvalidPassword = 4,
        InvalidUsername = 5,
        LoggedIn = 7
    }

    public enum PinResult : byte
    {
        Valid = 0,
        Register = 1,
        Invalid = 2,
        Error = 3,
        Request = 4,
        Cancel = 5
    }

    public enum VacResult : byte
    {
        CharInfo = 0,
        SendCount = 1,
        AlreadyLoggedIn = 2,
        UnknownError = 3,
        NoCharacters = 4
    }
    #endregion

    #region NPCs
    public enum NpcMessageType : byte
    {
        Standard,
        YesNo,
        RequestText,
        RequestNumber,
        Choice,
        RequestStyle = 7,
        AcceptDecline = 12
    }

    public enum ShopAction : byte
    {
        Buy,
        Sell,
        Recharge,
        Leave
    }

    public enum AdminShopAction : byte
    {
        Buy = 1,
        Exit = 2,
        Register = 3
    }

    public enum StorageAction : byte
    {
        Withdraw = 4,
        Deposit,
        Unknown,
        ModifyMeso,
        Leave
    }
    #endregion

    #region Quests
    public enum QuestAction : byte
    {
        RestoreLostItem,
        Start,
        Complete,
        Forfeit,
        ScriptStart,
        ScriptEnd
    }

    [Flags]
    public enum QuestFlags : short
    {
        //TODO: Test this; I'm just guessing
        AutoStart = 0x01,
        SelectedMob = 0x02
    }

    public enum QuestResult : byte
    {
        AddTimeLimit = 0x06,
        RemoveTimeLimit = 0x07,
        Complete = 0x08,
        GenericError = 0x09,
        NoInventorySpace = 0x0A,
        NotEnoughMesos = 0x0B,
        ItemWornByChar = 0x0D,
        OnlyOneOfItemAllowed = 0x0E,
        Expire = 0x0F,
        ResetTimeLimit = 0x10
    }

    public enum QuestStatus : byte
    {
        NotStarted = 0,
        InProgress = 1,
        Complete = 2
    }
    #endregion

    #region Reactors
    public enum ReactorEventType
    {
        PlainAdvanceState,
        HitFromLeft,
        HitFromRight,
        HitBySkill,
        NoClue, //NOTE: Applies to activate_by_touch reactors
        NoClue2, //NOTE: Applies to activate_by_touch reactors
        HitByItem,
        Timeout = 101
    }

    [Flags]
    public enum ReactorFlags : byte
    {
        //TODO: Test this; I'm just guessing
        FacesLeft = 0x01,
        ActivateByTouch = 0x02,
        RemoveInFieldSet = 0x04
    }
    #endregion

    #region Server
    public enum AccountLevel : byte
    {
        Normal,
        Intern,
        Gm,
        SuperGm,
        Administrator
    }

    public enum MessageType : byte
    {
        DropPickup,
        QuestRecord,
        CashItemExpire,
        IncreaseExp,
        IncreaseFame,
        IncreaseMeso,
        IncreaseGp,
        GiveBuff,
        GeneralItemExpire,
        System,
        QuestRecordEx,
        ItemProtectExpire,
        ItemExpireReplace,
        SkillExpire,
        TutorialMessage = 23
    }

    public enum ServerType
    {
        None,
        Login,
        Channel,
        Shop,
        Itc
    }

    public enum ScriptType
    {
        Npc,
        Portal
    }

    public enum NoticeType : byte
    {
        Notice,
        Popup,
        Megaphone,
        SuperMegaphone,
        Ticker,
        Pink,
        Blue,
        ItemMegaphone = 8
    }
    #endregion

    #region Skills and Buffs

    public enum PrimaryBuffStat : long
    {
        EnergyCharged = 0,
        DashSpeed = 1,
        DashJump = 2,
        RideVehicle = 3,
        PartyBooster = 4,
        GuidedBullet = 5,
        Undead = 6
    }
    #endregion

    #region Social
    public enum MessengerAction : byte
    {
        Open = 0,
        Join = 1,
        Leave = 2,
        Invite = 3,
        Note = 4,
        Decline = 5,
        Chat = 6
    }

    public enum MessengerResult : byte
    {
        Open = 0,
        Join = 1,
        Leave = 2,
        Invite = 3,
        Note = 4,
        Decline = 5,
        Chat = 6
    }

    public enum MultiChatType : byte
    {
        Buddy = 0,
        Party = 1,
        Guild = 2
    }

    public enum PartyAction : byte
    {
        Create = 1,
        Leave = 2,
        Join = 3,
        Invite = 4,
        Expel = 5,
        ChangeLeader = 6
    }

    public enum PartyResult : byte
    {
        Invite = 4,
        Update = 7,
        Create = 8,
        RemoveOrLeave = 12,
        Join = 15,
        ChangeLeader = 26
    }

    public enum GuildAction : byte
    {
        Update = 0,
        Create = 2,
        Invite = 5,
        Join = 6,
        Leave = 7,
        Expel = 8,
        ModifyTitles = 13,
        ModifyRank = 14,
        ModifyEmblem = 15,
        ModifyNotice = 16
    }

    public enum BbsAction : byte
    {
        AddOrEdit,
        Delete,
        List,
        View,
        Reply,
        DeleteReply
    }
    #endregion
}