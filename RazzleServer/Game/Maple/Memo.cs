using System;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Memo
    {
        public int Id { get; set; }
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public DateTime Received { get; private set; }

        public Memo(MemoEntity entity)
        {
            Id = entity.Id;
            Sender = entity.Sender;
            Message = entity.Message;
            Received = entity.Received;
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var item = dbContext.MemoEntities.Find(Id);
                if (item != null)
                {
                    dbContext.Remove(item);
                    dbContext.SaveChanges();
                }
            }
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {

                oPacket.WriteInt(Id);
                oPacket.WriteString(Sender + " "); // NOTE: Space is intentional.
                oPacket.WriteString(Message);
                oPacket.WriteDateTime(Received);
                oPacket.WriteByte(3); // TODO: Memo kind (0 - None, 1 - Fame, 2 - Gift).

                return oPacket.ToArray();
            }
        }
    }
}
