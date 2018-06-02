using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterKeymap : MapleKeyedCollection<KeymapKey, Shortcut>
    {
        private const int KeyCount = 90;

        public Character Parent { get; }

        public CharacterKeymap(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var entities = dbContext.KeyMaps.Where(x => x.CharacterId == Parent.Id);

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
                foreach (var entry in Values)
                {
                    dbContext.KeyMaps.Add(new KeyMapEntity
                    {
                        CharacterId = Parent.Id,
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
                var entities = dbContext.KeyMaps.Where(x => x.CharacterId == Parent.Id);

                dbContext.KeyMaps.RemoveRange(entities);
                dbContext.SaveChanges();
            }
        }

        public void Send()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.Keymap))
            {
                oPacket.WriteBool(false);

                for (var i = 0; i < KeyCount; i++)
                {
                    var key = (KeymapKey)i;

                    if (Contains(key))
                    {
                        var shortcut = this[key];

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

        public void CreateDefaultKeymap()
        {
            Clear();
            Add(new Shortcut(KeymapKey.One, KeymapAction.AllChat));
            Add(new Shortcut(KeymapKey.Two, KeymapAction.PartyChat));
            Add(new Shortcut(KeymapKey.Three, KeymapAction.BuddyChat));
            Add(new Shortcut(KeymapKey.Four, KeymapAction.GuildChat));
            Add(new Shortcut(KeymapKey.Six, KeymapAction.SpouseChat));
            Add(new Shortcut(KeymapKey.Q, KeymapAction.QuestMenu));
            Add(new Shortcut(KeymapKey.W, KeymapAction.WorldMap));
            Add(new Shortcut(KeymapKey.E, KeymapAction.EquipmentMenu));
            Add(new Shortcut(KeymapKey.R, KeymapAction.BuddyList));
            Add(new Shortcut(KeymapKey.I, KeymapAction.ItemMenu));
            Add(new Shortcut(KeymapKey.O, KeymapAction.PartySearch));
            Add(new Shortcut(KeymapKey.P, KeymapAction.PartyList));
            Add(new Shortcut(KeymapKey.BracketLeft, KeymapAction.Shortcut));
            Add(new Shortcut(KeymapKey.BracketRight, KeymapAction.QuickSlot));
            Add(new Shortcut(KeymapKey.LeftCtrl, KeymapAction.Attack));
            Add(new Shortcut(KeymapKey.S, KeymapAction.AbilityMenu));
            Add(new Shortcut(KeymapKey.G, KeymapAction.GuildList));
            Add(new Shortcut(KeymapKey.H, KeymapAction.WhisperChat));
            Add(new Shortcut(KeymapKey.K, KeymapAction.SkillMenu));
            Add(new Shortcut(KeymapKey.L, KeymapAction.QuestHelper));
            Add(new Shortcut(KeymapKey.Semicolon, KeymapAction.Medal));
            Add(new Shortcut(KeymapKey.Quote, KeymapAction.ExpandChat));
            Add(new Shortcut(KeymapKey.Backslash, KeymapAction.SetKey));
            Add(new Shortcut(KeymapKey.Z, KeymapAction.PickUp));
            Add(new Shortcut(KeymapKey.X, KeymapAction.Sit));
            Add(new Shortcut(KeymapKey.C, KeymapAction.Messenger));
            Add(new Shortcut(KeymapKey.M, KeymapAction.MiniMap));
            Add(new Shortcut(KeymapKey.LeftAlt, KeymapAction.Jump));
            Add(new Shortcut(KeymapKey.F1, KeymapAction.Cockeyed));
            Add(new Shortcut(KeymapKey.F2, KeymapAction.Happy));
            Add(new Shortcut(KeymapKey.F3, KeymapAction.Sarcastic));
            Add(new Shortcut(KeymapKey.F4, KeymapAction.Crying));
            Add(new Shortcut(KeymapKey.F5, KeymapAction.Outraged));
            Add(new Shortcut(KeymapKey.F6, KeymapAction.Shocked));
            Add(new Shortcut(KeymapKey.F7, KeymapAction.Annoyed));
        }

        public override KeymapKey GetKey(Shortcut item) => item.Key;
    }
}
