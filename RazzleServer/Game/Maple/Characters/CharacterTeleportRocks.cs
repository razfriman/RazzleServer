using System.Collections.Generic;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterTeleportRocks
    {
        public Character Parent { get; }

        public List<int> Maps { get; }

        public CharacterTeleportRocks(Character parent)
        {
            Parent = parent;

            Maps = new List<int>();
        }

        public void Load()
        {
            using (var context = new MapleDbContext())
            {
                // TODO - Load the 5 rock IDs from the database
            }
        }


        public void Save()
        {
            // TODO - Save the 5 rock IDs in the database
        }

        public bool Contains(int mapId) => Maps.Contains(mapId);

        public void Update(PacketReader iPacket)
        {
            var action = (TeleportRockAction)iPacket.ReadByte();
            var result = TeleportRockResult.Success;

            switch (action)
            {
                case TeleportRockAction.Remove:
                {
                    var mapId = iPacket.ReadInt();
                    if (!Maps.Contains(mapId))
                    {
                        return;
                    }

                    Maps.Remove(mapId);
                    result = TeleportRockResult.Delete;
                }
                    break;

                case TeleportRockAction.Add:
                {
                    var mapId = Parent.Map.MapleId;

                    // TODO: Check if the map field limits allow teleport rocks (e.g. Maple Island is forbidden).

                    if (true)
                    {
                        Maps.Add(mapId);
                    }

                    result = TeleportRockResult.Add;
                }
                    break;
            }

            SendRockUpdate(result);
        }

        public void SendRockUpdate(TeleportRockResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.TeleportRock))
            {
                pw.WriteByte((byte)result);

                if (result == TeleportRockResult.Add || result == TeleportRockResult.Delete)
                {
                    pw.WriteBytes(ToByteArray());
                }

                Parent.Client.Send(pw);
            }
        }

        public bool Use(int itemId, PacketReader packet)
        {
            var used = false;
            var action = packet.ReadByte();
            var destinationMapId = -1;
            var result = TeleportRockResult.Success;

            if (action == 0) // NOTE: Preset map.
            {
                var mapId = packet.ReadInt();

                if (!Parent.TeleportRocks.Contains(mapId))
                {
                    result = TeleportRockResult.CannotGo;
                }

                destinationMapId = mapId;
            }
            else if (action == 1) // NOTE: IGN.
            {
                var targetName = packet.ReadString();
                var target = Parent.Client.Server.GetCharacterByName(targetName);

                if (target == null)
                {
                    result = TeleportRockResult.DifficultToLocate;
                }
                else
                {
                    destinationMapId = target.Map.MapleId;
                }
            }

            if (destinationMapId != -1)
            {
                var originMap = Parent.Map;
                var destinationMap = DataProvider.Maps.Data[destinationMapId];

                // TODO: Field limit check.
                // TODO: Origin map field limit check.
                // TODO: Continent check.
                if (originMap.MapleId == destinationMap.MapleId)
                {
                    result = TeleportRockResult.AlreadyThere;
                }
            }

            if (result == TeleportRockResult.Success)
            {
                Parent.ChangeMap(destinationMapId);
                used = true;
            }
            else
            {
                SendRockUpdate(result);
            }

            return used;
        }

        public byte[] ToByteArray()
        {
            using (var pw = new PacketWriter())
            {
                for (var i = 0; i < 5; i++)
                {
                    pw.WriteInt(9 <= Maps.Count ? Maps[i] : 999999999);
                }

                return pw.ToArray();
            }
        }
    }
}
