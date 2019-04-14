using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;

namespace RazzleServer.Shop.Maple
{
    public sealed class ShopAccount
    {
        public ShopClient Client { get; }
        public int Id { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public Gender Gender { get; set; }
        public BanReasonType BanReason { get; set; }
        public bool IsMaster { get; set; }
        public bool IsOnline { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }

        public ShopAccount(ShopClient client) => Client = client;

        public ShopAccount(int accountId, ShopClient client)
        {
            Id = accountId;
            Client = client;
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
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
            BanReason = (BanReasonType)account.BanReason;
            IsMaster = account.IsMaster;
        }
    }
}
