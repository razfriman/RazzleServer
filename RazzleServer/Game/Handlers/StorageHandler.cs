using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Items;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.Storage)]
    public class StorageHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var action = (StorageAction)packet.ReadByte();

            switch (action)
            {
                case StorageAction.Withdraw:
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

                        using (var oPacket = new PacketWriter(ServerOperationCode.Storage))
                        {

                            oPacket.WriteByte(13);
                            oPacket.WriteByte(client.Character.Storage.Slots);
                            oPacket.WriteShort((short)(2 << (byte)item.Type));
                            oPacket.WriteShort(0);
                            oPacket.WriteInt(0);
                            oPacket.WriteByte((byte)itemsByType.Count);

                            foreach (var loopItem in itemsByType)
                            {
                                oPacket.WriteBytes(loopItem.ToByteArray(true, true));
                            }

                            client.Send(oPacket);
                        }
                    }
                    break;

                case StorageAction.Deposit:
                    {
                        var slot = packet.ReadShort();
                        var itemId = packet.ReadInt();
                        var quantity = packet.ReadShort();

                        var item = client.Character.Items[itemId, slot];

                        if (client.Character.Storage.IsFull)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Storage))
                            {
                                oPacket.WriteByte(17);

                                client.Send(oPacket);
                            }

                            return;
                        }

                        if (client.Character.Meso <= client.Character.Storage.Npc.StorageCost)
                        {
                            client.Character.Notify("You don't have enough meso to store the item.", NoticeType.Popup); // TOOD: Is there a packet for client.Character?

                            return;
                        }

                        client.Character.Meso -= client.Character.Storage.Npc.StorageCost;

                        client.Character.Items.Remove(item, true);

                        item.Parent = client.Character.Items; // NOTE: client.Character is needed because when we remove the item is sets parent to none.
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

                        using (var oPacket = new PacketWriter(ServerOperationCode.Storage))
                        {

                            oPacket.WriteByte(13);
                            oPacket.WriteByte(client.Character.Storage.Slots);
                            oPacket.WriteShort((short)(2 << (byte)item.Type));
                            oPacket.WriteShort(0);
                            oPacket.WriteInt(0);
                            oPacket.WriteByte((byte)itemsByType.Count);

                            foreach (var loopItem in itemsByType)
                            {
                                oPacket.WriteBytes(loopItem.ToByteArray(true, true));
                            }

                            client.Send(oPacket);
                        }
                    }
                    break;

                case StorageAction.ModifyMeso:
                    {
                        var meso = packet.ReadInt();

                        if (meso > 0) // NOTE: Withdraw meso.
                        {
                            // TODO: Meso checks.
                        }

                        client.Character.Meso -= meso;
                        client.Character.Meso += meso;

                        using (var oPacket = new PacketWriter(ServerOperationCode.Storage))
                        {

                            oPacket.WriteByte(19);
                            oPacket.WriteByte(client.Character.Storage.Slots);
                            oPacket.WriteShort(2);
                            oPacket.WriteShort(0);
                            oPacket.WriteInt(0);
                            oPacket.WriteInt(client.Character.Meso);

                            client.Send(oPacket);
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
