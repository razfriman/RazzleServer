using System;
using RazzleServer.Common;
using RazzleServer.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
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
            using var dbContext = new MapleDbContext();
            var item = dbContext.Memos.Find(Id);
            if (item != null)
            {
                dbContext.Remove(item);
                dbContext.SaveChanges();
            }
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteInt(Id);
            pw.WriteString($"{Sender} "); // NOTE: Space is intentional.
            pw.WriteString(Message);
            pw.WriteDateTime(Received);
            pw.WriteByte(3); // TODO: Memo kind (0 - None, 1 - Fame, 2 - Gift).

            return pw.ToArray();
        }
    }
}
