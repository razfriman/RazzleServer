﻿using System;

namespace RazzleServer.Game.Server
{
    public sealed class Migration
    {
        public string Host { get; }
        public int AccountId { get; }
        public int CharacterId { get; }
        public DateTime Expiry { get; }

        public Migration(string host, int accountId, int characterId)
        {
            Host = host;
            AccountId = accountId;
            CharacterId = characterId;
            Expiry = DateTime.UtcNow;
        }
    }
}
