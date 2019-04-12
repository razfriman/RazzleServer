using System;
using System.Linq;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CheckName)]
    public class CheckNameHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var error = name.Length < 4
                        || name.Length > 12
                        || client.Server.CharacterExists(name, client.World)
                        || DataProvider.CreationData.ForbiddenNames.Any(x => x.Equals(name, StringComparison.CurrentCultureIgnoreCase));

            using var pw = new PacketWriter(ServerOperationCode.CheckNameResult);
            pw.WriteString(name);
            pw.WriteBool(error);
            client.Send(pw);
        }
    }
}
