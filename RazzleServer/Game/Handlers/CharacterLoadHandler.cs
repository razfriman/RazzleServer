using RazzleServer.Center;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterLoad)]
    public class CharacterLoadHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            int characterId = packet.ReadInt();
            var accountId = client.Server.Manager.ValidateMigration(client.Host, characterId);

            if (accountId == 0)
            {
                client.Terminate("Invalid migration");
                return;
            }

            client.Account = new Account(accountId, client);
            client.Account.Load();

            client.Character = new Character(characterId, client);
            client.Character.Load();
            client.Character.Initialize();
        }
    }
}