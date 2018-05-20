using System;

namespace RazzleServer.Center.Maple
{
    public sealed class Migration
    {
        public string Host { get; private set; }
        public int AccountId { get; private set; }
        public int CharacterId { get; private set; }
        public DateTime Expiry { get; private set; }

        public Migration(string host, int accountId, int characterId)
        {
            Host = host;
            AccountId = accountId;
            CharacterId = characterId;
            Expiry = DateTime.Now;
        }
    }
}
