using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;

namespace RazzleServer.Game.Server
{
    public abstract class AMapleAccount
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public Gender Gender { get; set; }
        public bool IsMaster { get; set; }
        public BanReasonType BanReason { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }
        public bool IsOnline { get; set; }

        protected AMapleAccount() { }
        protected AMapleAccount(int accountId) => Id = accountId;

        public virtual void Load()
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
            Password = account.Password;
            Salt = account.Salt;
            Birthday = account.Birthday;
            Creation = account.Creation;
            BanReason = (BanReasonType)account.BanReason;
            IsMaster = account.IsMaster;
        }

        public virtual void Save()
        {
        }

        public virtual void Create()
        {
        }
    }
}
