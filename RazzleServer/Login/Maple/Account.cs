using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;
using RazzleServer.Data;

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
        private ILogger Log = LogManager.Log;

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
                    Log.LogError($"Cannot find account with Username: [{key}");
                    return;
                }

                this.ID = account.ID;
                this.Username = account.Username;
                this.Gender = (Gender)account.Gender;
                this.Password = account.Password;
                this.Salt = account.Salt;
                this.MaxCharacters = account.MaxCharacters;
            }
        }


        public void Save()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.FirstOrDefault(x => x.Username == Username);
                dbContext.SaveChanges();
            }
        
            //if (Assigned)
            //{
            //    datum.Update("ID = {0}", ID);
            //}
            //else
            //{
            //    ID = datum.InsertAndReturnID();
            //    Assigned = true;
            //}
        }

        public int Create()
        {
            using (var dbContext = new MapleDbContext())
            {
                var account = dbContext.Accounts.FirstOrDefault(x => x.Username == Username);
                dbContext.SaveChanges();
                return account.ID;
            }
        }
    }
}
