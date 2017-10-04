using Microsoft.Extensions.Logging;
using RazzleServer.Inventory;
using RazzleServer.Packet;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Util;
using MapleLib.PacketLib;

namespace RazzleServer.Player
{
    public class MapleTrade
    {
        private static ILogger Log = LogManager.Log;
        private static uint GlobalTradeID = 0;
        public static Dictionary<uint, MapleTrade> TradeIDs = new Dictionary<uint, MapleTrade>();//ID of trade | Trade
        public List<MapleCharacter> Partners = new List<MapleCharacter>();
        public TradeType Type;
        public uint TradeID;
        public MapleCharacter Owner;
        private List<TradeItem[]> items = new List<TradeItem[]>();

        public bool OwnerAccepted { get; set; }
        public bool PartnerAccepted { get; set; }
        public int OwnerMesos { get; set; }
        public int PartnerMesos { get; set; }

        private MapleTrade(TradeType type)
        {
            Type = type;
            TradeID = GlobalTradeID++;
            if (type == TradeType.Trade)
            {
                items.Add(new TradeItem[9]);
                items.Add(new TradeItem[9]);
            }
        }

        public static MapleTrade CreateTrade(TradeType type, MapleCharacter owner)
        {
            MapleTrade t = new MapleTrade(type) { Owner = owner };
            TradeIDs.Add(t.TradeID, t);
            owner.Client.SendPacket(t.GenerateTradeStart(owner, false));
            return t;
        }

        private static bool CanTransferItems(IEnumerable<TradeItem> list, MapleCharacter partner)
        {
            int equipCount = 0;
            int useCount = 0;
            int etcCount = 0;
            int setupCount = 0;
            foreach (TradeItem item in list)
            {
                if (item == null) continue;
                switch (item.Item.InventoryType)
                {
                    case MapleInventoryType.Equip:
                        equipCount++;
                        break;
                    case MapleInventoryType.Use:
                        useCount++;
                        break;
                    case MapleInventoryType.Etc:
                        etcCount++;
                        break;
                    case MapleInventoryType.Setup:
                        setupCount++;
                        break;
                }
            }
            if (partner.Inventory.GetFreeSlotCount(MapleInventoryType.Equip) < equipCount)
                return false;
            if (partner.Inventory.GetFreeSlotCount(MapleInventoryType.Use) < useCount)
                return false;
            if (partner.Inventory.GetFreeSlotCount(MapleInventoryType.Etc) < etcCount)
                return false;
            if (partner.Inventory.GetFreeSlotCount(MapleInventoryType.Setup) < setupCount)
                return false;
            return true;
        }

        private static bool TransferItems(TradeItem[] list, MapleCharacter owner, MapleCharacter partner)
        {
            if (!CanTransferItems(list, partner))
                return false;
            foreach (var item in list)
            {
                if (item == null) continue;
                MapleItem mitem = item.Item;
                MapleItem newItem;
                if (mitem.InventoryType == MapleInventoryType.Equip)
                    newItem = mitem;
                else
                    newItem = new MapleItem(mitem, item.Item.Source) { Quantity = item.Count };
                owner.Inventory.RemoveItemsFromSlot(mitem.InventoryType, mitem.Position, item.Count, false);
                partner.Inventory.AddItem(newItem, newItem.InventoryType);
            }
            return true;
        }

        public void AcceptTrade(MapleCharacter chr)
        {
            if (Type != TradeType.Trade) return;
            if (Partners.Count == 1 && chr == Owner)
            {
                OwnerAccepted = true;
                Partners[0].Client.SendPacket(GenerateTradeAccepted());
            }
            else if (Partners.Count == 1 && chr == Partners[0])
            {
                PartnerAccepted = true;
                Owner.Client.SendPacket(GenerateTradeAccepted());
            }
            else
            {
                Log.LogError($"Unable to accept trade because this character isn't registered as in the trade. Partner count [{Partners.Count}]");
                return;
            }
            if (OwnerAccepted && PartnerAccepted)
            {
                TradeItem[] ownersItems = items[0];
                TradeItem[] partnersItems = items[1];

                MapleCharacter partner = Partners[0];
                MapleCharacter owner = Owner;
                if (OwnerMesos + partner.Inventory.Mesos <= int.MaxValue && PartnerMesos + owner.Inventory.Mesos <= int.MaxValue &&
                    CanTransferItems(ownersItems, partner) && CanTransferItems(partnersItems, owner))
                {
                    Close(true, true);
                    TransferItems(ownersItems, owner, partner);
                    TransferItems(partnersItems, partner, owner);
                    Owner.Inventory.RemoveMesos(OwnerMesos, false);
                    partner.Inventory.RemoveMesos(PartnerMesos, false);

                    Owner.Inventory.GainMesos(PartnerMesos, false);
                    partner.Inventory.GainMesos(OwnerMesos, false);
                }
                else //Items cant be transfered
                {
                    Close(true, false);
                }
            }
        }

        public void Invite(MapleCharacter c, MapleCharacter inviter)
        {
            if (c.ActionState != ActionState.ENABLED || c.Trade != null)
            {
                inviter.Client.SendPacket(GenerateCharacterBusyMessage(c.Name));
                return;
            }
            c.Client.SendPacket(GenerateTradeInvite(inviter));
            c.Invites[InviteType.TRADE] = new Invite((int)TradeID, InviteType.TRADE);
        }

        public void Close(bool finished, bool success) //may be different for non-trade types like shops
        {
            Owner.Trade = null;
            if (Owner.Client != null)
            {
                Owner.Client.SendPacket(GenerateTradeClose(finished, success));
                MapleInventory.UpdateMesos(Owner.Client, Owner.Inventory.Mesos);
                if (!success || !finished)
                {
                    foreach (TradeItem item in items[0])
                    {
                        if (item != null)
                        {
                            Owner.Client.SendPacket(MapleInventory.Packets.AddItem(item.Item, item.Item.InventoryType, item.Item.Position)); //force the "hidden" items to reappear
                        }
                    }
                    Owner.EnableActions();
                }
                else
                    Owner.EnableActions(false);
            }
            foreach (MapleCharacter chr in Partners.Where(chr => chr.Trade == this))
            {
                chr.Client.SendPacket(GenerateTradeClose(finished, success));
                chr.Trade = null;
                MapleInventory.UpdateMesos(chr.Client, chr.Inventory.Mesos);
                if (Type == TradeType.Trade)
                {
                    if (!success || !finished)
                    {
                        foreach (TradeItem item in items[1])
                        {
                            if (item == null) continue;
                            chr.Client.SendPacket(MapleInventory.Packets.AddItem(item.Item, item.Item.InventoryType, item.Item.Position));
                        }
                    }
                }
                chr.EnableActions(false);
            }
            TradeIDs.Remove(TradeID);
        }

        public bool IsOwner(MapleCharacter c)
        {
            return (Owner == c);
        }

        public bool AddItem(MapleItem item, byte tradeSlot, short quantity, MapleCharacter itemOwner)
        {
            if (Partners.Count == 0) return false; //Partner hasn't accepted trade
            if (item.Tradeable)
                return false; //theyre probably hacking if they put in an untradable item too
            foreach (TradeItem[] arr in items)
            {
                if (arr.Any(t => t != null && t.Item == item)) //item is already added
                {
                    return false;
                }
            }
            if (Type == TradeType.Trade)
            {
                if (tradeSlot > 9 || tradeSlot < 1)
                    return false;//hacking
                if (Owner == itemOwner)
                {
                    items[0][tradeSlot] = new TradeItem(item, quantity);
                }
                else
                {
                    items[1][tradeSlot] = new TradeItem(item, quantity);
                }
                bool isOwner = Owner == itemOwner;
                Owner.Client.SendPacket(GenerateTradeItemAdd(item, !isOwner, tradeSlot));
                Partners[0].Client.SendPacket(GenerateTradeItemAdd(item, isOwner, tradeSlot));
                return true;
            }
            return false;
        }

        public void Chat(MapleCharacter talking, string text)
        {
            if (text.Length > 256) return;
            byte num = 0;//if the character isnt found in partners, it's the owner, so we dont check.
            for (byte i = 0; i < Partners.Count; i++)
            {
                if (Partners[i] == talking)
                    num = (byte)(i + 1);
            }
            if (Owner.Client?.Account.Character != null)
            {
                Owner.Client.SendPacket(GenerateChatMessage(talking.Name + " : " + text, num));
            }
            foreach (MapleCharacter chr in Partners)
            {
                if (chr.Trade == this)//be sure they accepted an invite
                {
                    chr.Client.SendPacket(GenerateChatMessage(talking.Name + " : " + text, num));
                }
            }
        }

        public void AddMesos(MapleCharacter chr, int mesos)
        {
            if (Type == TradeType.Trade && Partners.Count == 1 && Partners[0].Trade == this)
            {
                if (chr == Owner)
                {
                    if (OwnerMesos + mesos <= chr.Inventory.Mesos)
                    {
                        OwnerMesos += mesos;
                    }
                    Owner.Client.SendPacket(UpdateTradeMesos(OwnerMesos, 0));
                    Partners[0].Client.SendPacket(UpdateTradeMesos(OwnerMesos, 1));
                    MapleInventory.UpdateMesos(Owner.Client, Owner.Inventory.Mesos - OwnerMesos);
                }
                else if (chr == Partners[0])
                {
                    if (PartnerMesos + mesos <= chr.Inventory.Mesos)
                    {
                        PartnerMesos += mesos;
                    }
                    Owner.Client.SendPacket(UpdateTradeMesos(PartnerMesos, 1));
                    Partners[0].Client.SendPacket(UpdateTradeMesos(PartnerMesos, 0));
                    MapleInventory.UpdateMesos(Partners[0].Client, Partners[0].Inventory.Mesos - PartnerMesos);
                }
            }
        }
        public PacketWriter GenerateCharacterBusyMessage(string name)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x16);
            pw.WriteByte(0x02);
            pw.WriteByte(0x0);
            pw.WriteMapleString(name);
            return pw;
        }
        public PacketWriter UpdateTradeMesos(long mesos, byte character)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x04);
            pw.WriteByte(character);
            pw.WriteLong(mesos);
            return pw;
        }
        public PacketWriter GenerateChatMessage(string name, byte speakingCharacter)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x18);
            pw.WriteByte(0x19);
            pw.WriteByte(speakingCharacter);
            pw.WriteMapleString(name);
            return pw;
        }
        public PacketWriter GenerateTradeItemAdd(MapleItem item, bool mine, byte TradeSlot)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0);
            pw.WriteBool(mine);
            pw.WriteByte(TradeSlot);
            MapleItem.AddItemInfo(pw, item);
            return pw;
        }
        public PacketWriter GenerateRoomClosedMessage()
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x14);
            pw.WriteByte(0);
            pw.WriteByte(1);
            return pw;
        }
        public PacketWriter GenerateTradeAccepted()
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(8);
            return pw;
        }
        public PacketWriter GenerateTradeClose(bool Finished, bool success)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x1C);
            if (!Finished)
            {
                pw.WriteByte(1);
                pw.WriteByte(2);
            }
            else
            {
                pw.WriteByte(0);
                pw.WriteByte((byte)(success ? 0x07 : 0x08));
            }
            return pw;
        }
        public PacketWriter GenerateTradeDeny(MapleCharacter chr)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x16);
            pw.WriteByte(3);
            pw.WriteByte(0);
            pw.WriteMapleString(chr.Name);
            return pw;
        }
        public PacketWriter GenerateTradeInvite(MapleCharacter invitedBy)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x15);
            pw.WriteByte(4);
            pw.WriteMapleString(invitedBy.Name);
            pw.WriteUInt(TradeID);
            return pw;
        }
        public PacketWriter GenerateTradePartnerAdd(MapleCharacter c, byte position)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x13);
            pw.WriteByte(position);
            MapleCharacter.AddCharLook(pw, c, false);
            pw.WriteMapleString(c.Name);
            pw.WriteShort(c.Job);
            return pw;
        }
        public PacketWriter GenerateTradeStart(MapleCharacter c, bool includeOwner)
        {
            var pw = new PacketWriter((ushort)SMSGHeader.PLAYER_INTERACTION);
            pw.WriteByte(0x14);
            pw.WriteByte(4);
            pw.WriteByte(2);
            pw.WriteBool(includeOwner);
            if (includeOwner)
            {
                pw.WriteByte(0);
                MapleCharacter.AddCharLook(pw, Owner, false);
                pw.WriteMapleString(Owner.Name);
                pw.WriteShort(Owner.Job);
            }
            pw.WriteBool(includeOwner);
            MapleCharacter.AddCharLook(pw, c, false);
            pw.WriteMapleString(c.Name);
            pw.WriteShort(c.Job);
            pw.WriteByte(0xFF);
            return pw;
        }
    }
    public class TradeItem
    {
        public MapleItem Item { get; private set; }
        public short Count { get; private set; }
        public TradeItem(MapleItem item, short count)
        {
            Count = count;
            Item = item;
        }
    }
    public enum TradeType
    {
        Trade,
        Shop,
    }
}