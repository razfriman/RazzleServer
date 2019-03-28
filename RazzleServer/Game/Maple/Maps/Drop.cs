using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class Drop : MapObject, ISpawnable
    {
        public const int ExpiryTime = 60 * 1000;

        [JsonIgnore] public Character Owner { get; set; }
        [JsonIgnore] public Character Picker { get; set; }
        public Point Origin { get; set; }
        public Delay Expiry { get; set; }

        public DropType DropType { get; set; } = DropType.Normal;

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
            return GetInternalPacket(DropAnimationType.DropAnimation, null);
        }

        public PacketWriter GetCreatePacket(Character temporaryOwner)
        {
            return GetInternalPacket(DropAnimationType.DropAnimation, temporaryOwner);
        }

        public PacketWriter GetSpawnPacket()
        {
            return GetInternalPacket(DropAnimationType.ShowExisting, null);
        }

        public PacketWriter GetSpawnPacket(Character temporaryOwner)
        {
            return GetInternalPacket(DropAnimationType.ShowExisting, temporaryOwner);
        }

        private PacketWriter GetInternalPacket(DropAnimationType dropAnimationType, Character temporaryOwner)
        {
            var pw = new PacketWriter(ServerOperationCode.DropEnterField);


            pw.WriteByte((byte)(dropAnimationType));
            pw.WriteInt(ObjectId);
            pw.WriteBool(this is Meso);

            switch (this)
            {
                case Meso meso:
                    pw.WriteInt(meso.Amount);
                    break;
                case Item item:
                    pw.WriteInt(item.MapleId);
                    break;
            }

            pw.WriteInt(Owner?.Id ?? temporaryOwner.Id);
            pw.WriteByte((byte)DropType);
            pw.WritePoint(Position);
            pw.WriteInt(Dropper.ObjectId);

            if (dropAnimationType != DropAnimationType.ShowExisting)
            {
                pw.WritePoint(Origin);
                pw.WriteShort(0); // NOTE: Foothold or delay probably.
            }

            
            switch (this)
            {
                case Item item:
                    pw.WriteDateTime(item.Expiration);
                    break;
            }

            pw.WriteByte(0); // NOTE: Pet equip pick-up.

            return pw;
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
