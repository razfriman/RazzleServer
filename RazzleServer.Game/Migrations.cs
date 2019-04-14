using System;
using System.Linq;
using RazzleServer.Common.Util;

namespace RazzleServer.Game
{
    public sealed class Migrations : MapleKeyedCollection<string, Migration>
    {
        public int Validate(string host, int characterId)
        {
            var migration = Values
                .Where(x => x.Host == host)
                .FirstOrDefault(x => x.CharacterId == characterId);

            return RemoveMigration(migration);
        }

        private int RemoveMigration(Migration migration)
        {
            if (migration == null)
            {
                return 0;
            }

            Remove(migration);
            return (DateTime.UtcNow - migration.Expiry).TotalSeconds > 30 ? 0 : migration.AccountId;
        }

        public override string GetKey(Migration item) => item.Host;
    }
}
