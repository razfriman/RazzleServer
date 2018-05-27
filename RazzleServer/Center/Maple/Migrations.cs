using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RazzleServer.Center.Maple
{
    public sealed class Migrations : KeyedCollection<string, Migration>
    {
        public int Validate(string host, int characterId)
        {
            var migration = this
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

        protected override string GetKeyForItem(Migration item) => item.Host;
    }
}
