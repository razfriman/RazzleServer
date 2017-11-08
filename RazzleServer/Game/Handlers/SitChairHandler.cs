using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseChair)]
    public class SitChairHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int chairItemID = packet.ReadInt();

            if (!client.Character.Items.Contains(chairItemID))
            {
                return;
            }

            client.Character.Chair = chairItemID;

            using (var oPacket = new PacketWriter(ServerOperationCode.ShowChair))
            {
                oPacket.WriteInt(client.Character.ID);
                oPacket.WriteInt(chairItemID);
                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}

