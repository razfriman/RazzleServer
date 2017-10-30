using System;
namespace RazzleServer.Common.Constants
{
    public enum UserEffect : byte
    {
        LevelUp = 0,
        SkillUse = 1,
        SkillAffected = 2,
        Quest = 3,
        Pet = 4,
        SkillSpecial = 5,
        ProtectOnDieItemUse = 6,
        PlayPortalSE = 7,
        JobChanged = 8,
        QuestComplete = 9,
        IncDecHPEffect = 10,
        BuffItemEffect = 11,
        SquibEffect = 12,
        MonsterBookCardGet = 13,
        LotteryUse = 14,
        ItemLevelUp = 15,
        ItemMaker = 16,
        ExpItemConsumed = 17,
        ReservedEffect = 18,
        Buff = 19,
        ConsumeEffect = 20,
        UpgradeTombItemUse = 21,
        BattlefieldItemUse = 22,
        AvatarOriented = 23,
        IncubatorUse = 24,
        PlaySoundWithMuteBGM = 25,
        SoulStoneUse = 26,
        IncDecHPEffect_EX = 27,
        DeliveryQuestItemUse = 28, // NOTE: Post big bang update.
        RepeatEffectRemove = 29, // NOTE: Post big bang update.
        EvolRing = 30 // NOTE: Post big bang update.
    }
}
