using RazzleServer.Data;
using RazzleServer.Common.Packet;
using System.Linq;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.DELETE_CHAR)]
    public class DeleteCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pic = packet.ReadString();
            var characterID = packet.ReadInt();
        }
    }
}