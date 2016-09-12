using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Inventory
{
    public class MapleInventory
    {
        #region Init
        private readonly Dictionary<short, MapleItem> EquippedInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> EquipInventory = new Dictionary<short, MapleItem>();

        internal MapleEquip GetEquippedItem(short weapon)
        {
            throw new NotImplementedException();
        }

        private readonly Dictionary<short, MapleItem> UseInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> SetupInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> EtcInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> CashInventory = new Dictionary<short, MapleItem>();

        public byte EquipSlots { get; set; }
        public byte UseSlots { get; set; }
        public byte SetupSlots { get; set; }
        public byte EtcSlots { get; set; }
        public byte CashSlots { get; set; }

        private MapleCharacter Owner;

        public long Mesos
        {
            get
            {
                return Owner.Mesos;
            }
            private set
            {
                Owner.Mesos = value;
            }
        }

        public MapleInventory(MapleCharacter owner)
        {
            Owner = owner;

            EquipSlots = 24;
            UseSlots = 24;
            SetupSlots = 24;
            EtcSlots = 24;
            CashSlots = 96;
        }

        internal MapleItem GetFirstItemFromInventory(MapleInventoryType use, Func<MapleItem, bool> p)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Release()
        {
            Owner = null;
        }

        public void Bind(MapleCharacter character)
        {
            Owner = character;
        }

        internal void RemoveItemsFromSlot(MapleInventoryType inventoryType, short position, int v)
        {
            throw new NotImplementedException();
        }

        internal void RemoveItem(MapleInventoryType inventoryType, short position)
        {
            throw new NotImplementedException();
        }

        internal void AddItemById(int itemId, string v, short count)
        {
            throw new NotImplementedException();
        }

        internal void GainMesos(int data, bool v1, bool v2)
        {
            throw new NotImplementedException();
        }

        #region internal classes
        public class InventoryOperation
        {
            public MapleInventoryOperationType OperationType;
            public MapleInventoryType InventoryType;
            public short ItemPosition;
            public short NewPosition;
            public short Quantity;
            public MapleItem Item;

            public InventoryOperation(MapleInventoryOperationType operationType, MapleInventoryType inventoryType, short position)
            {
                OperationType = operationType;
                InventoryType = inventoryType;
                ItemPosition = position;
            }
        }

        internal void RemoveItemsById(int ammoItemId, int bulletCon, bool v)
        {
            throw new NotImplementedException();
        }

        internal bool HasItem(int key, int value)
        {
            throw new NotImplementedException();
        }
        #endregion

        public static class Packets
        {
            public static PacketWriter UpdateItemQuantity(MapleInventoryType inventoryType, short position, short quantity)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.UpdateQuantity, inventoryType, position) { Quantity = quantity } });
            }

            //Completely removes the item from inventory
            public static PacketWriter RemoveItem(MapleInventoryType inventoryType, short position)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Remove, inventoryType, position) });
            }

            public static PacketWriter MoveItem(MapleInventoryType type, short oldPosition, short newPosition)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Move, type, oldPosition) { NewPosition = newPosition } });
            }

            public static PacketWriter AddItem(MapleItem item, MapleInventoryType inventoryType, short position)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Add, inventoryType, position) { Item = item } });
            }

            //Client automatically swaps the items if the new position has an item in it
            public static PacketWriter ShowOperations(List<InventoryOperation> operations)
            {
                PacketWriter pw = new PacketWriter();
                pw.WriteHeader(SMSGHeader.INVENTORY_OPERATION);
                pw.WriteBool(true); //enable actions
                pw.WriteShort((short)operations.Count);

                foreach (InventoryOperation operation in operations)
                {
                    pw.WriteByte((byte)operation.OperationType);
                    pw.WriteByte((byte)operation.InventoryType);
                    switch (operation.OperationType)
                    {
                        case MapleInventoryOperationType.Add:
                            pw.WriteShort(operation.ItemPosition);
                            MapleItem.AddItemInfo(pw, operation.Item);
                            break;
                        case MapleInventoryOperationType.UpdateQuantity:
                            pw.WriteShort(operation.ItemPosition);
                            pw.WriteShort(operation.Quantity);
                            break;
                        case MapleInventoryOperationType.Move:
                            pw.WriteShort(operation.ItemPosition);
                            pw.WriteShort(operation.NewPosition);
                            if (operation.ItemPosition < 0 || operation.NewPosition < 0) //from or to equipped inventory
                                pw.WriteByte(0); //increments every time an equipped inventory operation is done in gMS
                            break;
                        case MapleInventoryOperationType.Remove:
                            pw.WriteShort(operation.ItemPosition);
                            if (operation.ItemPosition < 0) //from equipped inventory
                                pw.WriteByte(0); //increments every time an equipped inventory operation is done in gMS
                            break;
                    }
                }
                return pw;
            }

            public static PacketWriter ShowMesoGain(int amount, bool inChat)
            {
                PacketWriter pw = new PacketWriter();
                pw.WriteHeader(SMSGHeader.SHOW_STATUS_INFO);
                if (inChat)
                {
                    pw.WriteByte(6);
                    pw.WriteInt(amount);
                    pw.WriteInt(-1);
                }
                else
                {
                    pw.WriteByte(0);
                    pw.WriteByte(1);
                    pw.WriteByte(0);
                    pw.WriteInt(amount);
                    pw.WriteShort(0);
                    pw.WriteShort(0); //new v158
                }
                return pw;
            }

            public static PacketWriter ShowItemGain(int itemId, int quantity, bool inChat = false)
            {
                PacketWriter pw = new PacketWriter();
                pw.WriteHeader(SMSGHeader.SHOW_STATUS_INFO);
                pw.WriteByte(0);
                pw.WriteByte(0);
                pw.WriteInt(itemId);
                pw.WriteInt(quantity);
                return pw;
            }

            public static PacketWriter ShowInventoryFull()
            {
                PacketWriter pw = new PacketWriter();
                pw.WriteHeader(SMSGHeader.INVENTORY_OPERATION);
                pw.WriteByte(1);
                pw.WriteByte(0);
                pw.WriteByte(0);
                return pw;
            }

            public static void AddInventoryInfo(PacketWriter pw, MapleInventory inventory)
            {
                pw.WriteLong(inventory.Mesos); //long in v137 //pw.WriteLong(long.MaxValue); trolololo will go outside the meso box coz too many digits (max is 9,999,999,999)          
                pw.WriteInt(0);
                pw.WriteInt(inventory.Owner.ID);

                for (int i = 0; i < 7; i++)
                {
                    pw.WriteInt(0); //new v137, probably bit inventory
                }
                pw.WriteZeroBytes(3);

                pw.WriteByte(inventory.EquipSlots);
                pw.WriteByte(inventory.UseSlots);
                pw.WriteByte(inventory.SetupSlots);
                pw.WriteByte(inventory.EtcSlots);
                pw.WriteByte(inventory.CashSlots);

                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2L)); //TODO quest 122700 timestamp

                pw.WriteByte(0); // new v148

                //Equipped items
                Dictionary<short, MapleItem> equipped = inventory.GetInventory(MapleInventoryType.Equipped);
                foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -100 && kvp.Value.Position < 0))
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //1

                //Masked equipped items, Cash Shop stuff I think
                foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -1000 && kvp.Value.Position < -100))
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //2

                //Equip inventory
                Dictionary<short, MapleItem> equipInventory = inventory.GetInventory(MapleInventoryType.Equip);
                foreach (KeyValuePair<short, MapleItem> kvp in equipInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //3

                // Masked equipped pos <= -1000 && > -1100
                pw.WriteShort(0); //4

                // Masked equipped pos <= -1100 && > -1200
                pw.WriteShort(0); //5

                // Masked equipped pos <= 1200
                pw.WriteShort(0); //6

                // ?
                pw.WriteShort(0); //7

                // ?
                pw.WriteShort(0); //8

                //Totem slots, pos 5000
                pw.WriteShort(0); //9

                pw.WriteShort(0); //10 added v137

                pw.WriteShort(0); //11 added v137

                pw.WriteShort(0); //12 added v137

                pw.WriteShort(0); //13 added v142

                pw.WriteShort(0); //14 added v143

                pw.WriteShort(0); //15 added v143

                Dictionary<short, MapleItem> useInventory = inventory.GetInventory(MapleInventoryType.Use);
                foreach (KeyValuePair<short, MapleItem> kvp in useInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //16  

                Dictionary<short, MapleItem> setupInventory = inventory.GetInventory(MapleInventoryType.Setup);
                foreach (KeyValuePair<short, MapleItem> kvp in setupInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //17           

                Dictionary<short, MapleItem> etcInventory = inventory.GetInventory(MapleInventoryType.Etc);
                foreach (KeyValuePair<short, MapleItem> kvp in etcInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //18

                Dictionary<short, MapleItem> cashInventory = inventory.GetInventory(MapleInventoryType.Cash);
                foreach (KeyValuePair<short, MapleItem> kvp in cashInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //19       

                //TODO: extended slots
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteByte(0);
            }

            public static PacketWriter UpdateCharacterLook(MapleCharacter chr)
            {
                PacketWriter pw = new PacketWriter();
                pw.WriteHeader(SMSGHeader.UPDATE_CHAR_LOOK);
                pw.WriteInt(chr.ID);
                pw.WriteByte(1);
                MapleCharacter.AddCharLook(pw, chr, false);
                pw.WriteShort(0); //TODO: RINGS
                pw.WriteShort(0); //TODO: RINGS
                pw.WriteShort(0); //TODO: MRINGINFO
                pw.WriteInt(0);
                pw.WriteByte(0);
                return pw;
            }
        }

        private Dictionary<short, MapleItem> GetInventory(MapleInventoryType use)
        {
            throw new NotImplementedException();
        }
    }



    public enum MapleInventoryType : sbyte
    {
        Equipped = -1,
        Undefined = 0,
        Equip = 1,
        Use = 2,
        Setup = 3,
        Etc = 4,
        Cash = 5
    }

    public enum MapleInventoryOperationType : byte
    {
        Add = 0,
        UpdateQuantity = 1,
        Move = 2,
        Remove = 3
    }

    public enum MapleEquipPosition : short
    {
        Hat = -1,
        FaceAcc = -2,
        EyeAcc = -3,
        Earings = -4,
        Top = -5,
        Bottom = -6,
        Shoes = -7,
        Gloves = -8,
        Cape = -9,
        SecondWeapon = -10,
        Weapon = -11
    }
}