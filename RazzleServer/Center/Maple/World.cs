using Destiny.Network;
using System.Collections.ObjectModel;
using System.Linq;
using RazzleServer.Util;
using MapleLib.PacketLib;
using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Maple
{
    public sealed class World : KeyedCollection<byte, CenterClient>
    {
        public byte ID { get; private set; }
        public string Name { get; private set; }
        public ushort Port { get; private set; }
        public ushort ShopPort { get; private set; }
        public byte Channels { get; private set; }
        public string TickerMessage { get; private set; }
        public bool AllowMultiLeveling { get; private set; }
        public int ExperienceRate { get; private set; }
        public int QuestExperienceRate { get; private set; }
        public int PartyQuestExperienceRate { get; private set; }
        public int MesoRate { get; private set; }
        public int DropRate { get; private set; }

        private CenterClient shop;

        public CenterClient Shop
        {
            get => shop;
            set
            {
                shop = value;

                if (value != null)
                {
                    Shop.Port = ShopPort;
                }
            }
        }

        public CenterClient RandomChannel => this[Functions.Random(this.Count())];

        public bool IsFull => Count == Channels;

        public bool HasShop => Shop != null;

        internal World(PacketReader inPacket)
        {
            ID = inPacket.ReadByte();
            Name = inPacket.ReadString();
            Port = inPacket.ReadUShort();
            ShopPort = inPacket.ReadUShort();
            Channels = inPacket.ReadByte();
            TickerMessage = inPacket.ReadString();
            AllowMultiLeveling = inPacket.ReadBool();
            ExperienceRate = inPacket.ReadInt();
            QuestExperienceRate = inPacket.ReadInt();
            PartyQuestExperienceRate = inPacket.ReadInt();
            MesoRate = inPacket.ReadInt();
            DropRate = inPacket.ReadInt();
        }

        protected override void InsertItem(int index, CenterClient item)
        {
            item.ID = (byte)index;
            item.Port = (ushort)(Port + index);

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            foreach (CenterClient loopChannel in this)
            {
                if (loopChannel.ID > index)
                {
                    loopChannel.ID--;

                    var pw = new PacketWriter();
                    pw.WriteHeader(InteroperabilityOperationCode.UpdateChannelID);
                    pw.WriteByte(loopChannel.ID);
                    loopChannel.Send(pw);
                }
            }
        }

        protected override byte GetKeyForItem(CenterClient item) => item.ID;
    }
}
