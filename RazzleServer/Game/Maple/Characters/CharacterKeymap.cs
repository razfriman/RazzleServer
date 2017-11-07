using System.Collections.ObjectModel;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Data;
using RazzleServer.DB.Models;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterKeymap : KeyedCollection<KeymapKey, Shortcut>
    {
        private const int KeyCount = 90;

        public Character Parent { get; private set; }

        public CharacterKeymap(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var entities = dbContext.KeyMaps.Where(x => x.CharacterID == Parent.ID);

                foreach (var entity in entities)
                {
                    Add(new Shortcut(entity));
                }
            }
        }

        public void Save()
        {
            Delete();

            using (var dbContext = new MapleDbContext())
            {
                foreach (Shortcut entry in this)
                {
                    dbContext.KeyMaps.Add(new KeyMapEntity
                    {
                        CharacterID = Parent.ID,
                        Action = (byte)entry.Action,
                        Key = (byte)entry.Key,
                        Type = (byte)entry.Type
                    });
                }

                dbContext.SaveChanges();
            }
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var entities = dbContext.KeyMaps.Where(x => x.CharacterID == Parent.ID);

                dbContext.KeyMaps.RemoveRange(entities);
                dbContext.SaveChanges();
            }
        }

        public void Send()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Keymap))
            {
                oPacket.WriteBool(false);

                for (int i = 0; i < KeyCount; i++)
                {
                    KeymapKey key = (KeymapKey)i;

                    if (Contains(key))
                    {
                        Shortcut shortcut = this[key];

                        oPacket.WriteByte((byte)shortcut.Type);
                        oPacket.WriteInt((int)shortcut.Action);
                    }
                    else
                    {
                        oPacket.WriteByte(0);
                        oPacket.WriteInt(0);
                    }
                }

                Parent.Client.Send(oPacket);
            }
        }

        public void Change(PacketReader iPacket)
        {
            int mode = iPacket.ReadInt();
            int count = iPacket.ReadInt();

            if (mode == 0)
            {
                if (count == 0)
                {
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    KeymapKey key = (KeymapKey)iPacket.ReadInt();
                    KeymapType type = (KeymapType)iPacket.ReadByte();
                    KeymapAction action = (KeymapAction)iPacket.ReadInt();

                    if (Contains(key))
                    {
                        if (type == KeymapType.None)
                        {
                            Remove(key);
                        }
                        else
                        {
                            this[key].Type = type;
                            this[key].Action = action;
                        }
                    }
                    else
                    {
                        Add(new Shortcut(key, action, type));
                    }
                }
            }
            else if (mode == 1) // NOTE: Pet automatic mana potion.
            {

            }
            else if (mode == 2) // NOTE: Pet automatic mana potion.
            {

            }
        }

        protected override KeymapKey GetKeyForItem(Shortcut item) => item.Key;
    }
}
