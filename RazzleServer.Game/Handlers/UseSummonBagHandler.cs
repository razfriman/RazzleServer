using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSummonBag)]
    public class UseSummonBagHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.GameCharacter.Items[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            client.GameCharacter.Items.Remove(itemId, 1);

            foreach (var summon in item.Summons)
            {
                if (Functions.Random(0, 100) < summon.Item2 && CachedData.Mobs.Data.ContainsKey(summon.Item1))
                {
                    client.GameCharacter.Map.Mobs.Add(new Mob(summon.Item1, client.GameCharacter.Position));
                }
            }
        }
    }
}
