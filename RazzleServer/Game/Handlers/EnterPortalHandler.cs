using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.EnterPortal)]
    public class EnterPortalHandler : GamePacketHandler
    {
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

            ScriptProvider.Portals.Execute(portal, client.Character);
        }
    }
}