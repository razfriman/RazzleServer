using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterMemos : MapleKeyedCollection<int, Memo>
    {
        public Character Parent { get; }

        public CharacterMemos(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var memos = dbContext
                    .Memos
                    .Where(x => x.CharacterId == Parent.Id)
                    .ToArray();

                foreach (var memo in memos)
                {
                    Add(new Memo(memo));
                }
            }
        }

        public void Send()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            {
                oPacket.WriteByte((byte)MemoResult.Load);
                oPacket.WriteByte((byte)Count);

                foreach (var memo in Values)
                {
                    oPacket.WriteBytes(memo.ToByteArray());
                }

                Parent.Client.Send(oPacket);
            }
        }

        public override int GetKey(Memo item) => item.Id;

        internal bool Create(string targetName, string message)
        {
            //if (Parent.Client.Server.World.IsCharacterOnline(targetName))
            //{
            //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            //    {
            //        oPacket.WriteByte((byte)MemoResult.Error);
            //        oPacket.WriteByte((byte)MemoError.ReceiverOnline);
            //        client.Send(oPacket);
            //    }
            //}
            //else if (!Database.Exists("characters", "Name = {0}", targetName))
            //{
            //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            //    {
            //        oPacket.WriteByte((byte)MemoResult.Error);
            //        oPacket.WriteByte((byte)MemoError.ReceiverInvalidName);

            //        client.Send(oPacket);
            //    }
            //}
            //else if (isReceiverInboxFull) // TODO: Receiver's inbox is full. I believe the maximum amount is 5, but need to verify.
            //{
            //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            //    {
            //        oPacket.WriteByte((byte)MemoResult.Error);
            //        oPacket.WriteByte((byte)MemoError.ReceiverInboxFull);
            //        client.Send(oPacket);
            //    }
            //}
            //else
            //{
            //    Datum datum = new Datum("memos");

            //    datum["CharacterId"] = Database.Fetch("characters", "Id", "Name = {0}", targetName);
            //    datum["Sender"] = client.Character.Name;
            //    datum["Message"] = message;
            //    datum["Received"] = DateTime.Now;

            //    datum.Insert();

            //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
            //    {
            //        oPacket.WriteByte((byte)MemoResult.Sent);

            //        client.Send(oPacket);
            //    }
            //    return true;
            //}

            return false;
        }
    }
}