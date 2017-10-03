using RazzleServer.Packet;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CHAR_SELECT)]
    public class SelectCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pic = packet.ReadMapleString();
            var characterId = packet.ReadInt();
            var macs = packet.ReadMapleString();

            if (client.Account.HasCharacter(characterId) && ServerManager.ChannelServers.ContainsKey(client.Channel))
            {
                ushort port = ServerManager.ChannelServers[client.Channel].Port;
                client.Account.MigrationData.CharacterID = characterId;
                client.Account.MigrationData.Character = MapleCharacter.LoadFromDatabase(characterId, false);
                client.Account.MigrationData.Character.Hidden = client.Account.IsGM;
                MigrationWorker.EnqueueMigration(characterId, client.Account.MigrationData);

                client.SendPacket(ChannelIpPacket(port, characterId));
            }
        }

        public static PacketWriter ChannelIpPacket(ushort port, int characterId)
        {
            var pw = new PacketWriter(SMSGHeader.SERVER_IP);
            pw.WriteShort(0);
            pw.WriteBytes(new byte[] { 127, 0, 0, 1 }); // Localhost
            pw.WriteUShort(port);
            pw.WriteInt(characterId);
            pw.WriteZeroBytes(5);
            return pw;
        }
    }
}