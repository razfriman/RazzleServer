namespace RazzleServer.Common.Constants
{
    #region Quests
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
    #endregion
}