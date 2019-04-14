using System;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.NpcStorage)]
    public class StorageHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var action = (StorageAction)packet.ReadByte();

            switch (action)
            {
                case StorageAction.Remove:
                    HandleRemove(packet, client);
                    break;
                case StorageAction.Add:
                    HandleAdd(packet, client);
                    break;
                case StorageAction.Meso:
                    HandleMeso(packet, client);
                    break;
                case StorageAction.Leave:
                    client.GameCharacter.Save();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void HandleMeso(PacketReader packet, GameClient client)
        {
            var meso = packet.ReadInt();

            if (meso < 0)
            {
                // Deposit
                if (client.GameCharacter.PrimaryStats.Meso + meso < 0)
                {
                    client.GameCharacter.Storage.StorageError(StorageResult.NotEnoughMesos);
                    return;
                }
            }
            else
            {
                // Withdraw
                if (client.GameCharacter.Storage.Meso - meso < 0)
                {
                    client.GameCharacter.Storage.StorageError(StorageResult.NotEnoughMesos);
                    return;
                }
            }

            client.GameCharacter.Storage.Meso -= meso;
            client.GameCharacter.PrimaryStats.Meso += meso;
            client.GameCharacter.Storage.Update(StorageResult.ChangeMeso, StorageEncodeFlags.EncodeMesos);
        }

        private static void HandleAdd(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();
            var quantity = packet.ReadShort();

            var item = client.GameCharacter.Items[itemId, slot];

            if (item == null)
            {
                client.GameCharacter.Storage.StorageError(StorageResult.InventoryFullOrNot);
                client.GameCharacter.LogCheatWarning(CheatType.InvalidStorageUpdate);
                return;
            }

            if (client.GameCharacter.Storage.IsFull)
            {
                client.GameCharacter.Storage.StorageError(StorageResult.StorageIsFull);
                return;
            }

            if (client.GameCharacter.PrimaryStats.Meso < client.GameCharacter.Storage.Npc.CachedReference.StorageCost)
            {
                client.GameCharacter.Storage.StorageError(StorageResult.NotEnoughMesos);
                return;
            }

            client.GameCharacter.PrimaryStats.Meso -= client.GameCharacter.Storage.Npc.CachedReference.StorageCost;
            client.GameCharacter.Items.Remove(item, true);
            client.GameCharacter.Storage.Add(item);
        }

        private static void HandleRemove(PacketReader packet, GameClient client)
        {
            var type = (ItemType)packet.ReadByte();
            var slot = packet.ReadByte();

            var item = client.GameCharacter.Storage.Items
                .Where(x => x.Slot == slot)
                .FirstOrDefault(x => x.Type == type);

            if (item == null)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidStorageUpdate);
                client.GameCharacter.Storage.StorageError(StorageResult.InventoryFullOrNot);
                return;
            }

            if (client.GameCharacter.Items.IsFull(item.Type))
            {
                client.GameCharacter.Storage.StorageError(StorageResult.InventoryFullOrNot);
                return;
            }

            if (client.GameCharacter.PrimaryStats.Meso < client.GameCharacter.Storage.Npc.CachedReference.StorageCost)
            {
                client.GameCharacter.Storage.StorageError(StorageResult.NotEnoughMesos);
                return;
            }

            client.GameCharacter.PrimaryStats.Meso -= client.GameCharacter.Storage.Npc.CachedReference.StorageCost;
            client.GameCharacter.Storage.Remove(item);
            client.GameCharacter.Items.Add(item, forceGetSlot: true);
            item.Save();
        }
    }
}
