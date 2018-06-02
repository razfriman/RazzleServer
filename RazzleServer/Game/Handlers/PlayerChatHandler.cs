using RazzleServer.Center;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Scripts;

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