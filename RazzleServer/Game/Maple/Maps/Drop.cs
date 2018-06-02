using Newtonsoft.Json;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class Drop : MapObject, ISpawnable
    {
        public const int ExpiryTime = 60 * 1000;

        [JsonIgnore]
        public Character Owner { get; set; }
        [JsonIgnore]
        public Character Picker { get; set; }
        public Point Origin { get; set; }
        public Delay Expiry { get; set; }

        private MapObject _mDropper;

        [JsonIgnore]
        public MapObject Dropper
        {
            get => _mDropper;
            set
            {
                _mDropper = value;

                Origin = _mDropper.Position;
                Position = _mDropper.Map.Footholds.FindFloor(_mDropper.Position);
            }
        }

        public abstract PacketWriter GetShowGainPacket();

        public PacketWriter GetCreatePacket()
        {
            return GetInternalPacket(true, null);
        }

        public PacketWriter GetCreatePacket(Character temporaryOwner)
        {
            return GetInternalPacket(true, temporaryOwner);
        }

        public PacketWriter GetSpawnPacket()
        {
            return GetInternalPacket(false, null);
        }

        public PacketWriter GetSpawnPacket(Character temporaryOwner)
        {
            return GetInternalPacket(false, temporaryOwner);
        }

        private PacketWriter GetInternalPacket(bool dropped, Character temporaryOwner)
        {
            var oPacket = new PacketWriter(ServerOperationCode.DropEnterField);


			oPacket.WriteByte((byte)(dropped ? 1 : 2)); // TODO: Other types; 3 = disappearing, and 0 probably is something as well.
            oPacket.WriteInt(ObjectId);
            oPacket.WriteBool(this is Meso);

            if (this is Meso)
            {
                oPacket.WriteInt(((Meso)this).Amount);
            }
            else if (this is Item)
            {
                oPacket.WriteInt(((Item)this).MapleId);
            }

            oPacket.WriteInt(Owner?.Id ?? temporaryOwner.Id);
            oPacket.WriteByte(0); // TODO: Type implementation (0 - normal, 1 - party, 2 - FFA, 3 - explosive)
            oPacket.WriteShort(Position.X);
            oPacket.WriteShort(Position.Y);
            oPacket.WriteInt(Dropper.ObjectId);

            if (dropped)
            {
                oPacket.WriteShort(Origin.X);
                oPacket.WriteShort(Origin.Y);
                oPacket.WriteShort(0); // NOTE: Foothold, probably.
            }

            if (this is Item)
            {
                oPacket.WriteLong(0); // NOTE: Item expiration.
            }

            oPacket.WriteByte(0); // NOTE: Pet equip pick-up.

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.DropLeaveField);

            oPacket.WriteByte((byte)(Picker == null ? 0 : 2));
            oPacket.WriteInt(ObjectId);
            oPacket.WriteInt(Picker?.Id ?? 0);

            return oPacket;
        }
    }
}
