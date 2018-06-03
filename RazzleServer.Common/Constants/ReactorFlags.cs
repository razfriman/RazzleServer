using System;

namespace RazzleServer.Common.Constants
{
    #region Reactors

    [Flags]
    public enum ReactorFlags : byte
    {
        //TODO: Test this; I'm just guessing
        FacesLeft = 0x01,
        ActivateByTouch = 0x02,
        RemoveInFieldSet = 0x04
    }
    #endregion
}