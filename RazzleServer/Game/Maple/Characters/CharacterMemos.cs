using System.Collections.ObjectModel;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterMemos : KeyedCollection<int, Memo>
    {
        public Character Parent { get; private set; }

        public CharacterMemos(Character parent)
        {
            this.Parent = parent;
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("memos").Populate("CharacterID = {0}", this.Parent.ID))
            //{
            //    this.Add(new Memo(datum));
            //}
        }

        // NOTE: Memos are inserted straight into the database.
        // Therefore, there is no need for a save method.

        public void Handle(PacketReader iPacket)
        {
            var action = (MemoAction)iPacket.ReadByte();

            switch (action)
            {
                case MemoAction.Send:
                    {
                        // TODO: This is occured when you send a note from the Cash Shop.
                        // As we don't have Cash Shop implemented yet, this remains unhandled.
                    }
                    break;

                case MemoAction.Delete:
                    {
                        byte count = iPacket.ReadByte();
                        byte a = iPacket.ReadByte();
                        byte b = iPacket.ReadByte();

                        for (byte i = 0; i < count; i++)
                        {
                            int id = iPacket.ReadInt();

                            if (!this.Contains(id))
                            {
                                continue;
                            }

                            this[id].Delete();
                        }

                    }
                    break;
            }
        }

        public void Send()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            {
                oPacket.WriteByte((byte)MemoResult.Send);
                oPacket.WriteByte((byte)this.Count);

                foreach (Memo memo in this)
                {
                    oPacket.WriteBytes(memo.ToByteArray());
                }

                this.Parent.Client.Send(oPacket);
            }
        }

        protected override int GetKeyForItem(Memo item)
        {
            return item.ID;
        }
    }
}
