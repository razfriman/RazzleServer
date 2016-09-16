using RazzleServer.Inventory;
using RazzleServer.Packet;
using RazzleServer.Util;
using System;
using System.Drawing;

namespace RazzleServer.Map
{
    public class MapleMapItem
    {
        public int ObjectID { get; private set; }
        public int OwnerID { get; private set; }
        public int Meso { get; set; }
        public MapleDropType DropType { get; set; }
        public bool PlayerDrop { get; set; }
        public DateTime ExpireTime { get; set; }
        public DateTime FFATime { get; set; }
        public Point Position { get; private set; }
        public MapleItem Item { get; private set; }


        public MapleMapItem(int objectID, MapleItem item, Point position, int ownerId, MapleDropType dropType, bool playerDrop,
            int meso = 0, int ffa = 30000, int expiration = 120000)
        {
            ObjectID = objectID;
            Item = item;
            Position = position;
            OwnerID = ownerId > 0 ? ownerId : 0;
            DropType = dropType;
            PlayerDrop = playerDrop;
            Meso = meso;
            ExpireTime = DateTime.UtcNow.AddMilliseconds(expiration);
            FFATime = DateTime.UtcNow.AddMilliseconds(ffa);
        }

        public static class Packets
        {
            public static PacketWriter SpawnMapItem(MapleMapItem mapItem, Point sourcePosition, byte type, int ownerObjectId = 0)
            {
                
                var pw = new PacketWriter(SMSGHeader.DROP_ITEM_FROM_MAPOBJECT);

                bool meso = mapItem.Meso > 0;
                pw.WriteByte(0);
                pw.WriteByte(type); //0 = drop animation, 1 = visible, 2 = spawned, 3 = dissapearing
                pw.WriteInt(mapItem.ObjectID);
                pw.WriteBool(meso);
                pw.WriteZeroBytes(12);
                pw.WriteInt(meso ? mapItem.Meso : mapItem.Item.ItemId);
                pw.WriteInt(mapItem.OwnerID);
                pw.WriteByte((byte)mapItem.DropType);
                pw.WritePoint(mapItem.Position);
                pw.WriteInt(ownerObjectId); //Mob which dropped the item
                if (type != 2)
                {
                    pw.WritePoint(sourcePosition);
                    pw.WriteShort(0); //fh
                }
                pw.WriteBool(false);//boss-drop style fly added in v143
                pw.WriteShort(0);
                if (!meso)
                    pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-1));
                pw.WriteBool(!mapItem.PlayerDrop);
                pw.WriteZeroBytes(10);
                return pw;
            }


            public static PacketWriter RemoveMapItem(int objectId, byte animation, int characterId = 0, int petSlot = 0)
            {
                
                var pw = new PacketWriter(SMSGHeader.REMOVE_ITEM_FROM_MAP);

                pw.WriteByte(animation); //0 = face, 1 = instant, 2 = looted by player, 5 = looted by pet
                pw.WriteInt(objectId);
                if (animation >= 2)
                {
                    pw.WriteInt(characterId);
                    if (animation == 5)
                        pw.WriteInt(petSlot);
                }
                return pw;
            }
        }
    }
}
