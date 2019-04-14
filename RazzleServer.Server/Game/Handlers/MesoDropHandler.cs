using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MesoDrop)]
    public class MesoDropHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var amount = packet.ReadInt();

            if (amount > client.GameCharacter.PrimaryStats.Meso || amount < 10 || amount > 50000)
            {
                return;
            }

            client.GameCharacter.PrimaryStats.Meso -= amount;

            var mesoDrop = new Meso(amount) {Dropper = client.GameCharacter, Owner = null};

            client.GameCharacter.Map.Drops.Add(mesoDrop);
        }
    }
}
