using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;
using Serilog;

namespace RazzleServer.Login.Maple
{
    public sealed class LoginAccount
    {
        public LoginClient Client { get; }
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

        private readonly ILogger _log = Log.ForContext<LoginAccount>();

        public LoginAccount(LoginClient client)
        {
            Client = client;
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
            var account = dbContext.Accounts.FirstOrDefault(x => x.Username == Username);

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

        public void Save()
        {
            using var dbContext = new MapleDbContext();
            var account = dbContext.Accounts.Find(Id);

            if (account == null)
            {
                _log.Error($"Account does not exists with Id [{Id}]");
                return;
            }

            account.Username = Username;
            account.Salt = Salt;
            account.Password = Password;
            account.Gender = (byte)Gender;
            account.Birthday = Birthday;
            account.Creation = Creation;
            account.BanReason = (byte)BanReason;
            account.IsMaster = IsMaster;

            dbContext.SaveChanges();
        }

        public void Create()
        {
            using var dbContext = new MapleDbContext();
            var account = dbContext.Accounts.FirstOrDefault(x => x.Username == Username);

            if (account != null)
            {
                _log.Error($"Error creating account - account already exists with username [{Username}]");
                return;
            }

            account = new AccountEntity
            {
                Username = Username,
                Salt = Salt,
                Password = Password,
                Gender = (byte)Gender,
                Birthday = Birthday,
                Creation = Creation,
                BanReason = (byte)BanReason,
                IsMaster = IsMaster
            };

            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            Id = account.Id;
        }
    }
}
