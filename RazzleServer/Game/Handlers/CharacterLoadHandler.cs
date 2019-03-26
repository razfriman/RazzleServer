using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterLoad)]
    public class CharacterLoadHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var characterId = packet.ReadInt();
            var accountId = client.Server.Manager.ValidateMigration(client.Host, characterId);

            if (accountId == 0)
            {
                client.Terminate("Invalid migration");
                return;
            }

            client.Account = new GameAccount(accountId, client);
            client.Account.Load();

            client.Character = new Character(characterId, client);
            client.Character.Load();
            client.Character.Initialize();
        }
    }
}
