using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseChair)]
    public class SitChairHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var chairItemId = packet.ReadInt();

            if (!client.Character.Items.Contains(chairItemId))
            {
                return;
            }

            client.Character.Chair = chairItemId;

            using (var oPacket = new PacketWriter(ServerOperationCode.ShowChair))
            {
                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteInt(chairItemId);
                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}

