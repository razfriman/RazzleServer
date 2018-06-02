using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.EnterPortal)]
    public class EnterPortalHandler : GamePacketHandler
    {
        private readonly ILogger _log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var portals = packet.ReadByte();

            if (portals != client.Character.Portals)
            {
                return;
            }

            var label = packet.ReadString();

            var portal = client.Character.Map.Portals[label];

            if (portal == null)
            {
                return;
            }

            try
            {
                new APortalScript(portal, client.Character).Execute();
            }
            catch (Exception ex)
            {
                _log.LogError($"Script error: {ex}");
            }
        }
    }
}