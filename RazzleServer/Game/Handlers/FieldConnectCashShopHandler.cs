using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.FieldConnectCashShop)]
    public class FieldConnectCashShopHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            if (client.Character.Map.CachedReference.IsUnableToShop)
            {
                Portal.SendMapTransferResult(client.Character, MapTransferResult.CannotGo);
                return;
            }
            
            client.OpenCashShop();
            TaskRunner.Run(() => client.Terminate("Migrating to Cash Shop"), TimeSpan.FromSeconds(5));
        }
    }
}
