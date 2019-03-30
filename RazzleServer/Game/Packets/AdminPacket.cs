using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Packets
{
    public class AdminPacket
    {
        public static void Hide(GameClient client, bool hide) {
            using (var pw = new PacketWriter(ServerOperationCode.AdminResult))
            {
                pw.WriteByte(AdminResultType.Hide);
                pw.WriteBool(hide);
                client.Send(pw);
            }
        }

        public static void BanCharacterMessage(GameClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.BanMessage);
            pw.WriteByte(0);
            client.Send(pw);
        }

        public static void InvalidNameMessage(GameClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.InvalidName);
            pw.WriteByte(0x0A);
            client.Send(pw);
        }
        
        public static void NpcResult(GameClient client)
        {
            //format ; {string} : {string} = {string} 
            var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.NpcResult);
            pw.WriteString("");
            pw.WriteString("");
            pw.WriteString("");
            client.Send(pw);
        }
    }
}
