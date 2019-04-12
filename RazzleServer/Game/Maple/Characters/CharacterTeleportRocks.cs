using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

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
            using var context = new MapleDbContext();
            var rocks = context.TeleportRocks
                .Where(x => x.CharacterId == Parent.Id)
                .Take(5)
                .Select(x => x.MapId)
                .ToList();
            Maps.AddRange(rocks);
        }


        public void Save()
        {
            using var context = new MapleDbContext();
            var existing = context.TeleportRocks.Where(x => x.CharacterId == Parent.Id).ToArray();
            context.TeleportRocks.RemoveRange(existing);
            context.TeleportRocks.AddRange(Maps.Select(x => new TeleportRockEntity
            {
                CharacterId = Parent.Id, MapId = x
            }));
            context.SaveChanges();
        }

        public bool Contains(int mapId) => Maps.Contains(mapId);

        public void SendRockUpdate(TeleportRockResult result)
        {
            using var pw = new PacketWriter(ServerOperationCode.TeleportRock);
            pw.WriteByte(result);

            if (result == TeleportRockResult.Add || result == TeleportRockResult.Delete)
            {
                pw.WriteBytes(ToByteArray());
            }

            Parent.Send(pw);
        }

        public void Add(int mapId)
        {
            var map = DataProvider.Maps.Data[mapId];
            if (map.FieldLimit.HasFlag(FieldLimitFlags.TeleportItemLimit))
            {
                SendRockUpdate(TeleportRockResult.CannotGo);
                return;
            }

            Maps.Add(mapId);
            SendRockUpdate(TeleportRockResult.Add);
        }

        public void Remove(int mapId)
        {
            if (!Contains(mapId))
            {
                return;
            }

            Maps.Remove(mapId);
            SendRockUpdate(TeleportRockResult.Delete);
        }

        public bool Use(PacketReader packet)
        {
            var action = (TeleportRockUseAction)packet.ReadByte();
            int destinationMapId;

            switch (action)
            {
                case TeleportRockUseAction.ByMap:
                {
                    var mapId = packet.ReadInt();

                    if (!Parent.TeleportRocks.Contains(mapId))
                    {
                        SendRockUpdate(TeleportRockResult.AlreadyThere);
                        return false;
                    }

                    destinationMapId = mapId;
                    break;
                }

                case TeleportRockUseAction.ByPlayer:
                {
                    var targetName = packet.ReadString();
                    var target = Parent.Client.Server.GetCharacterByName(targetName);

                    if (target == null)
                    {
                        SendRockUpdate(TeleportRockResult.DifficultToLocate);
                        return false;
                    }

                    destinationMapId = target.Map.MapleId;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (destinationMapId != -1)
            {
                var originMap = Parent.Map;
                var destinationMap = DataProvider.Maps.Data[destinationMapId];

                if (originMap.MapleId == destinationMap.MapleId)
                {
                    SendRockUpdate(TeleportRockResult.AlreadyThere);
                    return false;
                }

                if (originMap.FieldLimit.HasFlag(FieldLimitFlags.TeleportItemLimit))
                {
                    SendRockUpdate(TeleportRockResult.CannotGo);
                    return false;
                }

                Parent.ChangeMap(destinationMapId);
                return true;
            }

            SendRockUpdate(TeleportRockResult.CannotGo);
            return false;
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            for (var i = 0; i < 5; i++)
            {
                pw.WriteInt(i < Maps.Count ? Maps[i] : 999999999);
            }

            return pw.ToArray();
        }
    }
}
