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

        private static void ProcessWhisper(PacketReader packet, GameClient client, string targetName,
            Maple.Characters.GameCharacter target)
        {
            var text = packet.ReadString();

            using var pw = new PacketWriter(ServerOperationCode.GroupMessage);
            pw.WriteByte(10);
            pw.WriteString(targetName);
            pw.WriteBool(target != null);
            client.Send(pw);

            if (target == null)
            {
                return;
            }

            using var pwTarget = new PacketWriter(ServerOperationCode.Whisper);
            pwTarget.WriteByte(18);
            pwTarget.WriteString(client.GameCharacter.Name);
            pwTarget.WriteByte(client.Server.ChannelId);
            pwTarget.WriteByte(0);
            pwTarget.WriteString(text);
            target.Send(pwTarget);
        }

        private static void ProcessFind(GameClient client, string targetName, Maple.Characters.GameCharacter target)
        {
            if (target == null)
            {
                using var pw = new PacketWriter(ServerOperationCode.Whisper);
                pw.WriteByte(0x0A);
                pw.WriteString(targetName);
                pw.WriteBool(false);
                client.Send(pw);
            }
            else
            {
                var isInSameChannel = client.Server.ChannelId == target.Client.Server.ChannelId;

                using var pw = new PacketWriter(ServerOperationCode.Whisper);
                pw.WriteByte(0x09);
                pw.WriteString(targetName);
                pw.WriteByte(isInSameChannel ? 1 : 3);
                pw.WriteInt(isInSameChannel ? target.Map.MapleId : target.Client.Server.ChannelId);
                pw.WriteInt(0); // NOTE: Unknown.
                pw.WriteInt(0); // NOTE: Unknown.
                client.Send(pw);
            }
        }
    }
}
