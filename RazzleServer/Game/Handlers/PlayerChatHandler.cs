using RazzleServer.Common;
using RazzleServer.Common.Server;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerChat)]
    public class PlayerChatHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var text = packet.ReadString();

            if (text.StartsWith(ServerConfig.Instance.CommandIndicator))
            {
                ScriptProvider.Commands.Execute(client.Character, text);
            }
            else
            {
                client.Character.Talk(text);
            }
        }
    }
}
