using System.Threading;
using Newtonsoft.Json;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class Drop : IMapObject, ISpawnable
    {
        public const int ExpiryTime = 60 * 1000;

        [JsonIgnore] public ICharacter Owner { get; set; }
        [JsonIgnore] public ICharacter Picker { get; set; }
        public Point Origin { get; set; }
        public CancellationTokenSource Expiry { get; set; }

        public DropType DropType { get; set; } = DropType.Normal;

        private IMapObject _mDropper;

        [JsonIgnore]
        public IMapObject Dropper
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

        public PacketWriter GetCreatePacket(GameCharacter temporaryOwner)
        {
            return GetInternalPacket(DropAnimationType.DropAnimation, temporaryOwner);
        }

        public PacketWriter GetSpawnPacket()
        {
            return GetInternalPacket(DropAnimationType.ShowExisting, null);
        }

        public PacketWriter GetSpawnPacket(GameCharacter temporaryOwner)
        {
            return GetInternalPacket(DropAnimationType.ShowExisting, temporaryOwner);
        }

        private PacketWriter GetInternalPacket(DropAnimationType dropAnimationType, GameCharacter temporaryOwner)
        {
            var pw = new PacketWriter(ServerOperationCode.DropEnterField);


            pw.WriteByte(dropAnimationType);
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
            pw.WriteByte(DropType);
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
            var pw = new PacketWriter(ServerOperationCode.DropLeaveField);

            pw.WriteByte(Picker == null ? 0 : 2);
            pw.WriteInt(ObjectId);
            pw.WriteInt(Picker?.Id ?? 0);

            return pw;
        }

        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
    }
}
