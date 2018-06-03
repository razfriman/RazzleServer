using System;

namespace RazzleServer.Common.Constants
{
    #region Quests

    [Flags]
    public enum QuestFlags : short
    {
        //TODO: Test this; I'm just guessing
        AutoStart = 0x01,
        SelectedMob = 0x02
    }
    #endregion
}