using RazzleServer.Net.Packet;
using RazzleServer.Shop.Maple;

namespace RazzleServer.Shop.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterLoad)]
    public class CharacterLoadHandler : ShopPacketHandler
    {
        public override void HandlePacket(PacketReader packet, ShopClient client)
        {
            var characterId = packet.ReadInt();
            var accountId = client.Server.Manager.ValidateMigration(client.Host, characterId);

            if (accountId == 0)
            {
                client.Terminate("Invalid migration");
                return;
            }

            client.Account = new ShopAccount(accountId, client);
            client.Account.Load();
            client.SetOnline(true);
            
            client.Character = new ShopCharacter(characterId, client);
            client.Character.Load();
            client.Character.Initialize();
        }
    }
}
