using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;
using RazzleServer.Game.Server;
using Serilog;

namespace RazzleServer.Login.Maple
{
    public sealed class LoginAccount : AMapleAccount
    {
        public LoginClient Client { get; }

        private readonly ILogger _log = Log.ForContext<LoginAccount>();

        public LoginAccount(LoginClient client) => Client = client;

        public override void Load()
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

        public override void Save()
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

        public override void Create()
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
