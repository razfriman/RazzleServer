using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.DataProvider;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterTeleportRocks
    {
        public ICharacter Parent { get; }

        public List<int> Maps { get; }

        public CharacterTeleportRocks(ICharacter parent)
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
            var map = CachedData.Maps.Data[mapId];
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
