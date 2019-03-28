using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CancelBuff)]
    public class CancelBuffHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var mapleId = packet.ReadInt();

            switch (mapleId)
            {
                // TODO: Handle special skills.

                default:
                    client.Character.Buffs.Remove(mapleId);
                    break;
            }
        }
    }
}
