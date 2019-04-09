﻿using System;
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
                    client.Character.Save();
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
                if (client.Character.PrimaryStats.Meso + meso < 0)
                {
                    client.Character.Storage.StorageError(StorageResult.NotEnoughMesos);
                    return;
                }
            }
            else
            {
                // Withdraw
                if (client.Character.Storage.Meso - meso < 0)
                {
                    client.Character.Storage.StorageError(StorageResult.NotEnoughMesos);
                    return;
                }
            }

            client.Character.Storage.Meso -= meso;
            client.Character.PrimaryStats.Meso += meso;
            client.Character.Storage.Update(StorageResult.ChangeMeso, StorageEncodeFlags.EncodeMesos);
        }

        private static void HandleAdd(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();
            var quantity = packet.ReadShort();

            var item = client.Character.Items[itemId, slot];

            if (item == null)
            {
                client.Character.Storage.StorageError(StorageResult.InventoryFullOrNot);
                client.Character.LogCheatWarning(CheatType.InvalidStorageUpdate);
                return;
            }

            if (client.Character.Storage.IsFull)
            {
                client.Character.Storage.StorageError(StorageResult.StorageIsFull);
                return;
            }

            if (client.Character.PrimaryStats.Meso < client.Character.Storage.Npc.CachedReference.StorageCost)
            {
                client.Character.Storage.StorageError(StorageResult.NotEnoughMesos);
                return;
            }

            client.Character.PrimaryStats.Meso -= client.Character.Storage.Npc.CachedReference.StorageCost;
            client.Character.Items.Remove(item, true);
            client.Character.Storage.Add(item);
        }

        private static void HandleRemove(PacketReader packet, GameClient client)
        {
            var type = (ItemType)packet.ReadByte();
            var slot = packet.ReadByte();

            var item = client.Character.Storage.Items
                .Where(x => x.Slot == slot)
                .FirstOrDefault(x => x.Type == type);

            if (item == null)
            {
                client.Character.LogCheatWarning(CheatType.InvalidStorageUpdate);
                client.Character.Storage.StorageError(StorageResult.InventoryFullOrNot);
                return;
            }

            if (client.Character.Items.IsFull(item.Type))
            {
                client.Character.Storage.StorageError(StorageResult.InventoryFullOrNot);
                return;
            }

            if (client.Character.PrimaryStats.Meso < client.Character.Storage.Npc.CachedReference.StorageCost)
            {
                client.Character.Storage.StorageError(StorageResult.NotEnoughMesos);
                return;
            }

            client.Character.PrimaryStats.Meso -= client.Character.Storage.Npc.CachedReference.StorageCost;
            client.Character.Storage.Remove(item);
            client.Character.Items.Add(item, forceGetSlot: true);
            item.Save();
        }
    }
}
