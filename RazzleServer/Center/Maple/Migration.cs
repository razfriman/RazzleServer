using System;

namespace RazzleServer.Center.Maple
{
    public sealed class Migration
    {
        public string Host { get; private set; }
        public int AccountID { get; private set; }
        public int CharacterID { get; private set; }
        public DateTime Expiry { get; private set; }

        public Migration(string host, int accountID, int characterID)
        {
            Host = host;
            AccountID = accountID;
            CharacterID = characterID;
            Expiry = DateTime.Now;
        }
    }
}
