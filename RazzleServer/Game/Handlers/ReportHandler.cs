using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ReportPlayer)]
    public class ReportHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (ReportType)packet.ReadByte();
            var victimName = packet.ReadString();
            packet.ReadByte(); // NOTE: Unknown.
            var description = packet.ReadString();

            ReportResult result;

            switch (type)
            {
                case ReportType.IllegalProgramUsage:
                {
                }
                    break;

                case ReportType.ConversationClaim:
                {
                    var chatLog = packet.ReadString();
                }
                    break;
            }

            result = ReportResult.Success;

            using (var oPacket = new PacketWriter(ServerOperationCode.SueCharacterResult))
            {
                oPacket.WriteByte((byte)result);
                client.Send(oPacket);
            }
        }
    }
}
