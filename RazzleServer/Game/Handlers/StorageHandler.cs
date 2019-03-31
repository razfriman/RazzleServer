using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
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
                {
                    var type = (ItemType)packet.ReadByte();
                    var slot = packet.ReadByte();

                    var item = client.Character.Storage.Items[slot];

                    if (item == null)
                    {
                        return;
                    }

                    client.Character.Storage.Items.Remove(item);
                    item.Delete();

                    item.IsStored = false;

                    client.Character.Items.Add(item, forceGetSlot: true);

                    var itemsByType = new List<Item>();

                    foreach (var loopItem in client.Character.Items)
                    {
                        if (loopItem.Type == item.Type)
                        {
                            itemsByType.Add(loopItem);
                        }
                    }

                    using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
                    {
                        pw.WriteByte(13);
                        pw.WriteByte(client.Character.Storage.Slots);
                        pw.WriteShort((short)(2 << (byte)item.Type));
                        pw.WriteShort(0);
                        pw.WriteInt(0);
                        pw.WriteByte((byte)itemsByType.Count);

                        foreach (var loopItem in itemsByType)
                        {
                            pw.WriteBytes(loopItem.ToByteArray(true, true));
                        }

                        client.Send(pw);
                    }
                }
                    break;

                case StorageAction.Add:
                {
                    var slot = packet.ReadShort();
                    var itemId = packet.ReadInt();
                    var quantity = packet.ReadShort();

                    var item = client.Character.Items[itemId, slot];

                    if (client.Character.Storage.IsFull)
                    {
                        using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
                        {
                            pw.WriteByte(17);

                            client.Send(pw);
                        }

                        return;
                    }

                    if (client.Character.PrimaryStats.Meso <= client.Character.Storage.Npc.CachedReference.StorageCost)
                    {
                        client.Character.Notify("You don't have enough meso to store the item.",
                            NoticeType.Popup); // TOOD: Is there a packet for client.Character?
                        return;
                    }

                    client.Character.PrimaryStats.Meso -= client.Character.Storage.Npc.CachedReference.StorageCost;

                    client.Character.Items.Remove(item, true);

                    item.Parent =
                        client.Character
                            .Items; // NOTE: client.Character is needed because when we remove the item is sets parent to none.
                    item.Slot = (short)client.Character.Storage.Items.Count;
                    item.IsStored = true;

                    client.Character.Items.Add(item);

                    var itemsByType = new List<Item>();

                    foreach (var loopItem in client.Character.Items)
                    {
                        if (loopItem.Type == item.Type)
                        {
                            itemsByType.Add(loopItem);
                        }
                    }

                    using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
                    {
                        pw.WriteByte(13);
                        pw.WriteByte(client.Character.Storage.Slots);
                        pw.WriteShort((short)(2 << (byte)item.Type));
                        pw.WriteShort(0);
                        pw.WriteInt(0);
                        pw.WriteByte((byte)itemsByType.Count);

                        foreach (var loopItem in itemsByType)
                        {
                            pw.WriteBytes(loopItem.ToByteArray(true, true));
                        }

                        client.Send(pw);
                    }
                }
                    break;

                case StorageAction.Meso:
                {
                    var meso = packet.ReadInt();

                    if (meso > 0) // NOTE: Withdraw meso.
                    {
                        // TODO: Meso checks.
                    }

                    client.Character.PrimaryStats.Meso -= meso;
                    client.Character.PrimaryStats.Meso += meso;

                    using (var pw = new PacketWriter(ServerOperationCode.StorageResult))
                    {
                        pw.WriteByte(19);
                        pw.WriteByte(client.Character.Storage.Slots);
                        pw.WriteShort(2);
                        pw.WriteShort(0);
                        pw.WriteInt(0);
                        pw.WriteInt(client.Character.PrimaryStats.Meso);

                        client.Send(pw);
                    }
                }
                    break;

                case StorageAction.Leave:
                {
                    client.Character.Save();
                }
                    break;
            }
        }
    }
}
