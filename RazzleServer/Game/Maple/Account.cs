using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Account
    {
        public GameClient Client { get; private set; }

        public int ID { get; set; }
        public string Username { get; set; }
        public Gender Gender { get; set; }
        public bool IsMaster { get; set; }
        public bool IsBanned { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }

        private bool Assigned { get; set; }

        public Account(int accountID, GameClient client)
        {
            ID = accountID;
            Client = client;
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.Find(ID);

                if (account == null)
                {
                    throw new NoAccountException();
                }

                ID = account.ID;
                Username = account.Username;
                Gender = (Gender)account.Gender;
                Birthday = account.Birthday;
                Creation = account.Creation;
                IsBanned = account.IsBanned;
                IsMaster = account.IsMaster;
            }
        }
    }
}
