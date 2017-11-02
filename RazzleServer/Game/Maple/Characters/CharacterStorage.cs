using System.Collections.Generic;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterStorage
    {
        public Character Parent { get; private set; }
        public Npc Npc { get; private set; }
        public byte Slots { get; private set; }
        public int Meso { get; private set; }
        public List<Item> Items { get; set; }

        public bool IsFull => Items.Count == Slots;

        public CharacterStorage(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            Datum datum = new Datum("storages");

            try
            {
                datum.Populate("AccountID = {0}", this.Parent.AccountID);
            }
            catch
            {
                datum["AccountID"] = this.Parent.AccountID;
                datum["Slots"] = (byte)4;
                datum["Meso"] = 0;

                datum.Insert();
            }

            this.Slots = (byte)datum["Slots"];
            this.Meso = (int)datum["Meso"];

            this.Items = new List<Item>();

            foreach (Datum itemDatum in new Datums("items").Populate("AccountID = {0} AND IsStored = True", this.Parent.AccountID))
            {
                this.Items.Add(new Item(itemDatum));
            }
        }

        public void Save()
        {
            Datum datum = new Datum("storages");

            datum["Slots"] = this.Slots;
            datum["Meso"] = this.Meso;

            datum.Update("AccountID = {0}", this.Parent.AccountID);

            foreach (Item item in this.Items)
            {
                item.Save();
            }
        }

        public void Show(Npc npc)
        {
            this.Npc = npc;

            this.Load();

            using (var oPacket = new PacketWriter(ServerOperationCode.Storage))
            {
                oPacket.WriteByte(22);
                oPacket.WriteInt(npc.MapleID);
                oPacket.WriteByte(this.Slots);
                oPacket.WriteShort(126);
                oPacket.WriteShort(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt(this.Meso);
                oPacket.WriteShort(0);
                oPacket.WriteByte((byte)this.Items.Count);

                foreach (Item item in this.Items)
                {
                    oPacket.WriteBytes(item.ToByteArray(true, true));
                }

                oPacket.WriteShort(0);
                oPacket.WriteByte(0);

                this.Parent.Client.Send(oPacket);
            }
        }
    }
}
