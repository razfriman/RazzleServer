using System.Collections.ObjectModel;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterMemos : KeyedCollection<int, Memo>
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
                    .MemoEntities
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
                oPacket.WriteByte((byte)MemoResult.Send);
                oPacket.WriteByte((byte)Count);

                foreach (var memo in this)
                {
                    oPacket.WriteBytes(memo.ToByteArray());
                }

                Parent.Client.Send(oPacket);
            }
        }

        protected override int GetKeyForItem(Memo item) => item.Id;
    }
}
