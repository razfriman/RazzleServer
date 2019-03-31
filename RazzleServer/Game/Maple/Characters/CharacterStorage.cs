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
        public List<Item> Items { get; set; }

        public bool IsFull => Items.Count == Slots;

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

        private CharacterStorageEntity GenerateDefault() =>
            new CharacterStorageEntity {AccountId = Parent.AccountId, Slots = 4, Meso = 0};

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
                pw.WriteByte(Slots);
                pw.WriteShort(0x7E);
                pw.WriteInt(Meso);

                pw.WriteByte((byte)Items.Count);
                Items.ForEach(item => item.ToByteArray(true, true));
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
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
    }
}
