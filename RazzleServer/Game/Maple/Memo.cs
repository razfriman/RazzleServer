using System;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Memo
    {
        public int Id { get; set; }
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public DateTime Received { get; private set; }

        //public Memo(Datum datum)
        //{
        //    this.Id = (int)datum["Id"];
        //    this.Sender = (string)datum["Sender"];
        //    this.Message = (string)datum["Message"];
        //    this.Received = (DateTime)datum["Received"];
        //}

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
