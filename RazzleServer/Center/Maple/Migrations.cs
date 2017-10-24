using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RazzleServer.Center.Maple
{
    public sealed class Migrations : KeyedCollection<string, Migration>
    {
        public int Validate(string host, int characterID)
        {
            var migration = this
                .Where(x => x.Host == host)
                .FirstOrDefault(x => x.CharacterID == characterID);

            if (migration != null)
            {
                Remove(migration);

                if ((DateTime.Now - migration.Expiry).TotalSeconds > 30)
                {
                    return 0;
                }

                return migration.AccountID;
            }

            return 0;
        }

        protected override string GetKeyForItem(Migration item) => item.Host;
    }
}
