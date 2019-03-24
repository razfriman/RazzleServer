using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CheckName)]
    public class CheckNameHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var characterExists = client.Server.CharacterExists(name, client.World);

            using (var pw = new PacketWriter(ServerOperationCode.CheckNameResult))
            {
                pw.WriteString(name);
                pw.WriteBool(characterExists);
                client.Send(pw);
            }
        }
    }
}
