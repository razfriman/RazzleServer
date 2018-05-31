using System;

namespace RazzleServer.Data
{
    public class MemoEntity
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public DateTime Received { get; private set; }
    }
}
