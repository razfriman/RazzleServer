using RazzleServer.Center;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterLoad)]
    public class CharacterLoadHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int characterID = packet.ReadInt();

            client.Character = new Character(characterID, client);
            client.Character.Load();
            client.Character.Initialize();
        }
    }
}