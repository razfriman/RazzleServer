using System.ComponentModel;

namespace RazzleServer.Common.Constants
{
    public enum ServerRegistrationResponse : byte
    {
        Valid,
        [Description("Unknown server type.")]
        InvalidType,
        [Description("The provided security code is not corresponding.")]
        InvalidCode,
        [Description("Cannot register as all the spots are occupied.")]
        Full
    }

}
