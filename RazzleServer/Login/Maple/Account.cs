using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.DB.Models;

namespace RazzleServer.Login.Maple
{
    public sealed class Account : IMapleSavable
    {
        public LoginClient Client { get; private set; }

        public int ID { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public Gender Gender { get; set; }
        public string Pin { get; set; }
        public bool IsBanned { get; set; }
        public bool IsMaster { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }
        public int MaxCharacters { get; set; }
        private bool Assigned { get; set; }
        private readonly ILogger Log = LogManager.Log;

        public Account(LoginClient client)
        {
            Client = client;
        }

        public void Load(object key)
        {
            var username = key as string;

            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.FirstOrDefault(x => x.Username == username);

                if (account == null)
                {
                    throw new NoAccountException();
                }

                ID = account.ID;
                Username = account.Username;
                Gender = (Gender)account.Gender;
                Password = account.Password;
                Salt = account.Salt;
                MaxCharacters = account.MaxCharacters;
                Birthday = account.Birthday;
                Creation = account.Creation;
                Pin = account.Pin;
                IsBanned = account.IsBanned;
                IsMaster = account.IsMaster;
            }
        }


        public void Save()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.Find(ID);

                if (account == null)
                {
                    Log.LogError($"Account does not exists with ID [{ID}]");
                }

                account.Username = Username;
                account.Salt = Salt;
                account.Password = Password;
                account.Gender = (byte)Gender;
                account.Pin = Pin;
                account.Birthday = Birthday;
                account.Creation = Creation;
                account.IsBanned = IsBanned;
                account.IsMaster = IsMaster;
                account.MaxCharacters = MaxCharacters;

                dbContext.SaveChanges();
            }
        }

        public void Create()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.FirstOrDefault(x => x.Username == Username);

                if (account != null)
                {
                    Log.LogError($"Error creating acconut - account already exists with username [{Username}]");
                    return;
                }

                account = new AccountEntity
                {
                    Username = Username,
                    Salt = Salt,
                    Password = Password,
                    Gender = (byte)Gender,
                    Pin = Pin,
                    Birthday = Birthday,
                    Creation = Creation,
                    IsBanned = IsBanned,
                    IsMaster = IsMaster,
                    MaxCharacters = MaxCharacters
                };

                dbContext.Accounts.Add(account);
                dbContext.SaveChanges();
                ID = account.ID;
            }
        }
    }
}
