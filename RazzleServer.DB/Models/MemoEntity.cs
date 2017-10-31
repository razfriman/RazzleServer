using System;
namespace RazzleServer.DB.Models
{
    public class MemoEntity
    {
        public int ID { get; set; }
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public DateTime Received { get; private set; }
    }
}
