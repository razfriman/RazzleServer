using System;
using System.Linq;
using RazzleServer.Common.Util;

namespace RazzleServer.Center.Maple
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
            if (migration != null)
            {
                Remove(migration);

                return (DateTime.Now - migration.Expiry).TotalSeconds > 30 
                    ? 0 
                    : migration.AccountId;
            }

            return 0;
        }

        public override string GetKey(Migration item) => item.Host;
    }
}
