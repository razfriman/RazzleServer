using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.PLAYER_LOGGEDIN)]
    public class CharacterLoggedInHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            int characterId = packet.ReadInt();

            MigrationData migration = MigrationWorker.TryDequeueMigration(characterId, client.Channel);
            if (migration?.Character != null)
            {
                MapleAccount account = MapleAccount.GetAccountFromDatabase(migration.AccountName);
                MapleCharacter chr = migration.Character;

                account.Character = chr;
                client.Account = account;
                chr.Bind(client);
                account.MigrationData = migration;
                account.Character = chr;
                if (!migration.ToCashShop)
                {
                    var map = ServerManager.GetChannelServer(client.Channel).GetMap(chr.MapID);
                    if (map != null)
                    {
                        chr.Map = map;
                        MapleCharacter.EnterChannel(client);
                        client.Account.Character.LoggedIn();

                        //chr.Stats.Recalculate(chr);
                        chr.Position = map.GetStartpoint(0).Position;

                        map.AddCharacter(chr);
                    }
                }
                else
                {
                    //CashShop.StartShowing(client);
                }
            }
            else
            {
                client.Disconnect("No migration data found.");
            }
        }
    }
}

