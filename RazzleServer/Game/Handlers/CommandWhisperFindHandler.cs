using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CommandWhisperFind)]
    public class CommandWhisperFindHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (CommandType)packet.ReadByte();
            var targetName = packet.ReadString();
            var target = client.Server.GetCharacterByName(targetName);

            switch (type)
            {
                case CommandType.Find:
                    {
                        ProcessFind(client, targetName, target);
                    }
                    break;

                case CommandType.Whisper:
                    {
                        ProcessWhisper(packet, client, targetName, target);
                    }
                    break;
            }
        }

        private static void ProcessWhisper(PacketReader packet, GameClient client, string targetName, Maple.Characters.Character target)
        {
            var text = packet.ReadString();

            using (var oPacket = new PacketWriter(ServerOperationCode.GroupMessage))
            {
                oPacket.WriteByte(10);
                oPacket.WriteString(targetName);
                oPacket.WriteBool(target != null);
                client.Send(oPacket);
            }

            if (target != null)
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                {
                    oPacket.WriteByte(18);
                    oPacket.WriteString(client.Character.Name);
                    oPacket.WriteByte(client.Server.ChannelId);
                    oPacket.WriteByte(0);
                    oPacket.WriteString(text);
                    target.Client.Send(oPacket);
                }
            }
        }

        private static void ProcessFind(GameClient client, string targetName, Maple.Characters.Character target)
        {
            if (target == null)
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                {
                    oPacket.WriteByte(0x0A);
                    oPacket.WriteString(targetName);
                    oPacket.WriteBool(false);
                    client.Send(oPacket);
                }
            }
            else
            {
                var isInSameChannel = client.Server.ChannelId == target.Client.Server.ChannelId;

                using (var oPacket = new PacketWriter(ServerOperationCode.Whisper))
                {
                    oPacket.WriteByte(0x09);
                    oPacket.WriteString(targetName);
                    oPacket.WriteByte((byte)(isInSameChannel ? 1 : 3));
                    oPacket.WriteInt(isInSameChannel ? target.Map.MapleId : target.Client.Server.ChannelId);
                    oPacket.WriteInt(0); // NOTE: Unknown.
                    oPacket.WriteInt(0); // NOTE: Unknown.
                    client.Send(oPacket);
                }
            }
        }
    }
}
