using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSummonBag)]
    public class UseSummonBagHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt(); // NOTE: Ticks.
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.Character.Items[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            client.Character.Items.Remove(itemId, 1);

            foreach (var summon in item.Summons)
            {
                if (Functions.Random(0, 100) < summon.Item2 && DataProvider.Mobs.Data.ContainsKey(summon.Item1))
                {
                    client.Character.Map.Mobs.Add(new Mob(summon.Item1, client.Character.Position));
                }
            }
        }
    }
}