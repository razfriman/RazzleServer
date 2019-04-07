using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum StorageEncodeFlags
    {
        EncodeMesos = 0x02,
        EncodeInventoryEquip = 0x04,
        EncodeInventoryUse = 0x08,
        EncodeInventorySetUp = 0x10,
        EncodeInventoryEtc = EncodeInventoryEquip, // FIX for old versions, put in 0x20 when client is fixed
        EncodeInventoryPet = 0x40, // Cash in newer versions

        EncodeAll = EncodeMesos | EncodeInventoryEquip | EncodeInventoryUse | EncodeInventorySetUp |
                    EncodeInventoryEtc | EncodeInventoryPet,
    }
}
