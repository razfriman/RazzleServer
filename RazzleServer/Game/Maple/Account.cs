using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Exceptions;

namespace RazzleServer.Game.Maple
{
    public sealed class Account
    {
        public GameClient Client { get; private set; }

        public int Id { get; set; }
        public string Username { get; set; }
        public Gender Gender { get; set; }
        public bool IsMaster { get; set; }
        public bool IsBanned { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }

        private bool Assigned { get; set; }

        public Account(int accountId, GameClient client)
        {
            Id = accountId;
            Client = client;
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.Find(Id);

                if (account == null)
                {
                    throw new NoAccountException();
                }

                Id = account.Id;
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
