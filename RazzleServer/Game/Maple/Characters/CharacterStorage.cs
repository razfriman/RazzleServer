using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterStorage
    {
        public Character Parent { get; }
        public Npc Npc { get; private set; }
        public byte Slots { get; private set; }
        public int Meso { get; private set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public bool IsFull => Items.Count == Slots;

        public List<Item> ItemsByType(ItemType type) => Parent.Storage.Items.Where(x => x.Type == type).ToList();
        public List<Item> ItemsByType(byte type) => ItemsByType((ItemType)type);

        public CharacterStorage(Character parent) => Parent = parent;

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var entity = dbContext.CharacterStorages.FirstOrDefault(x => x.AccountId == Parent.AccountId);

                if (entity == null)
                {
                    entity = GenerateDefault();
                    dbContext.CharacterStorages.Add(entity);
                    dbContext.SaveChanges();
                }

                Slots = entity.Slots;
                Meso = entity.Meso;

                var itemEntities = dbContext.Items
                    .Where(x => x.AccountId == Parent.AccountId)
                    .Where(x => x.IsStored)
                    .ToList();

                itemEntities.ForEach(x => Items.Add(new Item(x)));
            }
        }

        private AccountStorageEntity GenerateDefault() =>
            new AccountStorageEntity {AccountId = Parent.AccountId, Slots = 4, Meso = 0};

        public void Save()
        {
            using (var dbContext = new MapleDbContext())
            {
                var entity = dbContext.CharacterStorages.FirstOrDefault(x => x.AccountId == Parent.AccountId);
                entity.Slots = Slots;
                entity.Meso = Meso;
                dbContext.SaveChanges();
            }

            Items.ForEach(item => item.Save());
        }

        public void Show(Npc npc)
        {
            Npc = npc;

            Load();

            using (var pw = new PacketWriter(ServerOperationCode.StorageShow))
            {
                pw.WriteInt(npc.MapleId);
                pw.WriteBytes(EncodeStorage(StorageEncodeFlags.EncodeAll));
                Parent.Client.Send(pw);
            }
        }

        public void StorageError(StorageResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
            {
                pw.WriteByte(result);
                Parent.Client.Send(pw);
            }
        }


        public void Update(StorageResult result, StorageEncodeFlags flags)
        {
            using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
            {
                pw.WriteByte(result);
                pw.WriteBytes(EncodeStorage(flags));
                Parent.Send(pw);
            }
        }

        private byte[] EncodeStorage(StorageEncodeFlags flags)
        {
            var packet = new PacketWriter();
            packet.WriteByte(Parent.Storage.Slots);
            packet.WriteShort((short)flags);

            if (flags.HasFlag(StorageEncodeFlags.EncodeMesos))
            {
                packet.WriteInt(Parent.Storage.Meso);
            }

            for (byte i = 1; i <= 5; i++)
            {
                var flag = GetEncodeFlagForInventory((ItemType)i);
                if (!flags.HasFlag(flag))
                {
                    continue;
                }

                var itemsInInventory = ItemsByType(i);
                packet.WriteByte((byte)itemsInInventory.Count);
                itemsInInventory.ForEach(item => packet.WriteBytes(item.ToByteArray(true, true)));
            }

            return packet.ToArray();
        }

        public static StorageEncodeFlags GetEncodeFlagForInventory(ItemType inventory)
        {
            StorageEncodeFlags flag;
            switch (inventory)
            {
                case ItemType.Equipment:
                    flag = StorageEncodeFlags.EncodeInventoryEquip;
                    break;
                case ItemType.Usable:
                    flag = StorageEncodeFlags.EncodeInventoryUse;
                    break;
                case ItemType.Setup:
                    flag = StorageEncodeFlags.EncodeInventorySetUp;
                    break;
                case ItemType.Etcetera:
                    flag = StorageEncodeFlags.EncodeInventoryEtc;
                    break;
                case ItemType.Pet:
                    flag = StorageEncodeFlags.EncodeInventoryPet;
                    break;
                default:
                    flag = 0;
                    break;
            }

            return flag;
        }
    }
}
