using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MultiChat)]
    public class MultiChatHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (MultiChatType)packet.ReadByte();
            var count = packet.ReadByte();

            var recipients = new List<int>();

            while (count-- > 0)
            {
                var recipientId = packet.ReadInt();

                recipients.Add(recipientId);
            }

            var text = packet.ReadString();

            switch (type)
            {
                case MultiChatType.Buddy:
                {
                }
                    break;

                case MultiChatType.Party:
                {
                }
                    break;
            }

            using (var pw = new PacketWriter(ServerOperationCode.GroupMessage))
            {
                pw.WriteByte(type);
                pw.WriteString(client.Character.Name);
                pw.WriteString(text);

                foreach (var recipient in recipients)
                {
                    client.Server.GetCharacterById(recipient).Client.Send(pw);
                }
            }
        }
    }
}
