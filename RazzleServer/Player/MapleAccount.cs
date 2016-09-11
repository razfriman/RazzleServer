using RazzleServer.Data;
using RazzleServer.DB.Models;
using RazzleServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Player
{
    public class MapleAccount : Account
    {
        public MigrationData MigrationData;

        private object ReleaseLock = new object();

        public MapleCharacter Character { get; set; }

        MapleAccount(int id)
        {
            ID = id;
        }

        public static MapleAccount FromEntity(Account entity)
        {
            if (entity == null) return null;

            MapleAccount dto = new MapleAccount(entity.ID)
            {
                Name = entity.Name,
                Password = entity.Password,
                AccountType = entity.AccountType,
                MaplePoints = entity.MaplePoints,
                NXCredit = entity.NXCredit,
                NXPrepaid = entity.NXPrepaid,
                Gender = entity.Gender,
                CharacterSlots = entity.CharacterSlots
            };

            return dto;
        }

        public static MapleAccount GetAccountFromDatabase(string name)
        {
            try
            {
                Account account;
                using (MapleDbContext context = new MapleDbContext())
                {
                    account = context.Accounts.SingleOrDefault(x => x.Name.ToLower().Equals(name.ToLower()));
                }
                return FromEntity(account);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static MapleAccount CreateAccount(string name, string password)
        {
            using (var context = new MapleDbContext())
            {
                if (!context.Accounts.Any(x => x.Name.ToLower() == name.ToLower()))
                {
                    var account = new Account
                    {
                        Name = name,
                        Password = password,
                        CharacterSlots = 6
                    };

                    context.Accounts.Add(account);
                    context.SaveChanges();

                    return FromEntity(account);
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsGM
        {
            get
            {
                return AccountType >= 2;
            }
        }

        public bool IsAdmin
        {
            get
            {
                return AccountType >= 3;
            }
        }

        public void Release()
        {
            lock (ReleaseLock)
            {
                if (Character != null)
                {
                    bool hasMigration = MigrationWorker.MigrationExists(Character.ID);
                    MapleCharacter.SaveToDatabase(Character);
                    Character.Release(hasMigration);
                    Character = null;
                }
                MigrationData = null;
            }
        }

        public List<MapleCharacter> GetCharsFromDatabase()
        {
            List<Character> DbChars;
            using (MapleDbContext DBContext = new MapleDbContext())
            {
                DbChars = DBContext.Characters.Where(x => x.AccountID == ID).ToList();
            }
            List<MapleCharacter> ret = new List<MapleCharacter>();
            foreach (Character DbChar in DbChars)
            {
                MapleCharacter chr = MapleCharacter.LoadFromDatabase(DbChar.ID, true);
                if (chr != null)
                    ret.Add(chr);
            }
            return ret;
        }

        public bool HasCharacter(int characterId)
        {
            using (MapleDbContext DBContext = new MapleDbContext())
            {
                Character DbChar = DBContext.Characters.SingleOrDefault(x => x.ID == characterId);
                return DbChar != null;
            }
        }

        public void SetPic(string pic)
        {
            using (MapleDbContext context = new MapleDbContext())
            {
                var account = context.Accounts.SingleOrDefault(x => x.ID == ID);
                account.Pic = pic;
                context.SaveChanges();
            }
        }
        public bool CheckPic(string enteredPic)
        {
            using (var context = new MapleDbContext())
            {
                var account = context.Accounts.SingleOrDefault(x => x.ID == ID);
                if (account?.Pic == null)
                    return false;
                return enteredPic == account.Pic;
            }
        }
        public bool HasPic()
        {
            Account DbAccount;
            using (MapleDbContext DBContext = new MapleDbContext())
            {
                DbAccount = DBContext.Accounts.SingleOrDefault(x => x.ID == ID);
            }
            try
            {
                if (DbAccount.Pic.Length != 0)
                    return true;
            }
            catch (Exception) { }
            return false;

        }

        public static bool AccountExists(string name)
        {
            using (MapleDbContext DBContext = new MapleDbContext())
            {
                return DBContext.Accounts.SingleOrDefault(x => x.Name.ToLower().Equals(name.ToLower())) != null;
            }
        }

        public bool CheckPassword(string enteredPassword)
        {
            Account DbAccount;
            using (MapleDbContext DBContext = new MapleDbContext())
            {
                DbAccount = DBContext.Accounts.SingleOrDefault(x => x.ID == ID);
            }
            if (DbAccount == null) return false;
            return enteredPassword == DbAccount.Password;
        }
    }
}