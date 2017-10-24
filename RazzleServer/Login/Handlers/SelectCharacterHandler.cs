using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.CHAR_SELECT)]
    public class SelectCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pic = packet.ReadString();
            var characterId = packet.ReadInt();
            var macs = packet.ReadString();

            if (client.Account.HasCharacter(characterId) && ServerManager.ChannelServers.ContainsKey(client.Channel))
            {
                ushort port = ServerManager.ChannelServers[client.Channel].Port;
                client.Account.MigrationData.CharacterID = characterId;
                client.Account.MigrationData.Character = MapleCharacter.LoadFromDatabase(characterId, false);
                client.Account.MigrationData.Character.Hidden = client.Account.IsGM;
                MigrationWorker.EnqueueMigration(characterId, client.Account.MigrationData);

                client.Send(ChannelIpPacket(port, characterId, client.Socket.HostBytes));
            }
        }

        public static PacketWriter ChannelIpPacket(ushort port, int characterId, byte[] host)
        {
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SERVER_IP);
            pw.WriteShort(0);
            pw.WriteBytes(host);
            pw.WriteUShort(port);
            pw.WriteInt(characterId);
            pw.WriteZeroBytes(5);
            return pw;
        }
    }
}