using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class Drop : MapObject, ISpawnable
    {
        public const int ExpiryTime = 60 * 1000;

        public Character Owner { get; set; }
        public Character Picker { get; set; }
        public Point Origin { get; set; }
        public Delay Expiry { get; set; }

        private MapObject mDropper;

        public MapObject Dropper
        {
            get
            {
                return mDropper;
            }
            set
            {
                mDropper = value;

                this.Origin = mDropper.Position;
                this.Position = mDropper.Map.Footholds.FindFloor(mDropper.Position);
            }
        }

        public abstract PacketWriter GetShowGainPacket();

        public PacketWriter GetCreatePacket()
        {
            return this.GetInternalPacket(true, null);
        }

        public PacketWriter GetCreatePacket(Character temporaryOwner)
        {
            return this.GetInternalPacket(true, temporaryOwner);
        }

        public PacketWriter GetSpawnPacket()
        {
            return this.GetInternalPacket(false, null);
        }

        public PacketWriter GetSpawnPacket(Character temporaryOwner)
        {
            return this.GetInternalPacket(false, temporaryOwner);
        }

        private PacketWriter GetInternalPacket(bool dropped, Character temporaryOwner)
        {
            var oPacket = new PacketWriter(ServerOperationCode.DropEnterField);

            oPacket.WriteByte((byte)(dropped ? 1 : 2)); // TODO: Other types; 3 = disappearing, and 0 probably is something as well.
            oPacket.WriteInt(this.ObjectId);
            oPacket.WriteBool(this is Meso);

            if (this is Meso)
            {
                oPacket.WriteInt(((Meso)this).Amount);
            }
            else if (this is Item)
            {
                oPacket.WriteInt(((Item)this).MapleId);
            }

            oPacket.WriteInt(this.Owner != null ? this.Owner.Id : temporaryOwner.Id);
            oPacket.WriteByte(0); // TODO: Type implementation (0 - normal, 1 - party, 2 - FFA, 3 - explosive)
            oPacket.WriteShort(this.Position.X);
            oPacket.WriteShort(this.Position.Y);
            oPacket.WriteInt(this.Dropper.ObjectId);

            if (dropped)
            {
                oPacket.WriteShort(this.Origin.X);
                oPacket.WriteShort(this.Origin.Y);
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

            oPacket.WriteByte((byte)(this.Picker == null ? 0 : 2));
            oPacket.WriteInt(this.ObjectId);
            oPacket.WriteInt(this.Picker != null ? this.Picker.Id : 0);

            return oPacket;
        }
    }
}
