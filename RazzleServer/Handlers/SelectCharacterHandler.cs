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

        //movement
        //29 00 01 4C 64 68 2A 81 00 7A 00 04 00 81 00 7A 00 00 00 00 00 01 00 05 1E 00 00 6C 00 7A 00 83 FF 00 00 01 00 03 D2 00 00 62 00 7A 00 FB FF 00 00 01 00 05 96 00 00 63 00 7A 00 00 00 00 00 01 00 05 78 00 11 80 88 88 88 00 00 00 20 02 62 00 7A 00 81 00 7A 00

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