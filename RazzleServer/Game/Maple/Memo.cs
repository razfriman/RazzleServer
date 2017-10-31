using System;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Memo
    {
        public int ID { get; set; }
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public DateTime Received { get; private set; }

        public Memo(Datum datum)
        {
            this.ID = (int)datum["ID"];
            this.Sender = (string)datum["Sender"];
            this.Message = (string)datum["Message"];
            this.Received = (DateTime)datum["Received"];
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var item = dbContext.MemoEntities.Find(ID);
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

                oPacket.WriteInt(this.ID);
                oPacket.WriteString(this.Sender + " "); // NOTE: Space is intentional.
                oPacket.WriteString(this.Message);
                oPacket.WriteDateTime(this.Received);
                oPacket.WriteByte(3); // TODO: Memo kind (0 - None, 1 - Fame, 2 - Gift).

                return oPacket.ToArray();
            }
        }
    }
}
