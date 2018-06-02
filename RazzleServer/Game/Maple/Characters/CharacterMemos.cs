using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;

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

        public bool Create(string targetName, string message)
        {
            if (Parent.Client.Server.World.GetCharacterByName(targetName) != null)
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                {
                    oPacket.WriteByte((byte)MemoResult.Error);
                    oPacket.WriteByte((byte)MemoError.ReceiverOnline);
                    Parent.Client.Send(oPacket);
                }
            }
            else if (!IsCharacterExists(targetName, Parent.WorldId))
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                {
                    oPacket.WriteByte((byte)MemoResult.Error);
                    oPacket.WriteByte((byte)MemoError.ReceiverInvalidName);

                    Parent.Client.Send(oPacket);
                }
            }
            else if (IsReceiverInboxFull(targetName, Parent.WorldId)) // TODO: Receiver's inbox is full. I believe the maximum amount is 5, but need to verify.
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                {
                    oPacket.WriteByte((byte)MemoResult.Error);
                    oPacket.WriteByte((byte)MemoError.ReceiverInboxFull);
                    Parent.Client.Send(oPacket);
                }
            }
            else
            {
                var targetId = GetCharacterIdByName(targetName, Parent.WorldId);
                Insert(message, targetId);

                using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                {
                    oPacket.WriteByte((byte)MemoResult.Sent);
                    Parent.Client.Send(oPacket);
                }
                return true;
            }

            return false;
        }

        private bool IsCharacterExists(string targetName, byte worldId)
        {
            using (var dbContext = new MapleDbContext())
            {
                return dbContext
                  .Characters
                  .Where(x => x.WorldId == worldId)
                    .Any(x => x.Name == targetName);
            }
        }

        private bool IsReceiverInboxFull(string targetName, byte worldId)
        {
            var id = GetCharacterIdByName(targetName, worldId);
            using (var dbContext = new MapleDbContext())
            {
                return dbContext
                    .Memos
                    .Count(x => x.CharacterId == id) > 5;
            }
        }

        private int GetCharacterIdByName(string targetName, byte worldId)
        {
            using (var dbContext = new MapleDbContext())
            {
                return dbContext
                    .Characters
                    .Where(x => x.WorldId == worldId)
                    .Where(x => x.Name == targetName)
                    .Select(x => x.Id)
                    .FirstOrDefault();
            }
        }

        private void Insert(string message, int targetId)
        {
            using (var dbContext = new MapleDbContext())
            {
                dbContext.Memos.Add(new MemoEntity
                {
                    CharacterId = targetId,
                    Message = message,
                    Received = DateTime.Now,
                    Sender = Parent.Name
                });
                dbContext.SaveChanges();
            }
        }
    }
}