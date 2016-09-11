using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Server
{
    public class MigrationWorker
    {
        private static readonly ExpiringDictionary<int, MigrationData> MigrationQueue = new ExpiringDictionary<int, MigrationData>(new TimeSpan(0, 2, 0));
        private static readonly object MigrationLock = new object();

        public static bool MigrationExists(int id)
        {
            lock (MigrationLock)
            {
                return MigrationQueue.ContainsKey(id);
            }
        }

        public static void EnqueueMigration(int id, MigrationData data)
        {
            lock (MigrationLock)
            {
                MigrationQueue.Add(id, data);
            }
        }
        /// <summary>
        /// Safely tries to dequeue a migration.
        /// </summary>
        public static MigrationData TryDequeueMigration(int id, byte channel)
        {
            lock (MigrationLock)
            {
                MigrationData connection = null;
                if (MigrationQueue.TryGetValue(id, out connection))
                {
                    if (connection.ToChannel == channel)
                    {
                        MigrationQueue.Remove(id);
                        return connection;
                    }
                }
            }
            return null;
        }
    }
}
