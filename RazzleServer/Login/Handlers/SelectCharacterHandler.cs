using RazzleServer.Common.Packet;
using RazzleServer.Server;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CHAR_SELECT)]
    public class SelectCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pic = packet.ReadString();
            var characterId = packet.ReadInt();
            var macs = packet.ReadString();
        }

        public static PacketWriter ChannelIpPacket(ushort port, int characterId, byte[] host)
        {
            
            var pw = new PacketWriter(ServerOperationCode.SERVER_IP);
            pw.WriteShort(0);
            pw.WriteBytes(host);
            pw.WriteUShort(port);
            pw.WriteInt(characterId);
            pw.WriteZeroBytes(5);
            return pw;
        }
    }
}