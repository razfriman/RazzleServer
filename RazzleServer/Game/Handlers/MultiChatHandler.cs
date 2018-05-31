using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MultiChat)]
    public class MultiChatHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (MultiChatType) packet.ReadByte();
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

                case MultiChatType.Guild:
                    {

                    }
                    break;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.GroupMessage))
            {
                oPacket.WriteByte((byte)type);
                oPacket.WriteString(client.Character.Name);
                oPacket.WriteString(text);

                foreach (var recipient in recipients)
                {
                    client.Server.GetCharacterById(recipient).Client.Send(oPacket);
                }
            }
        }
    }
}