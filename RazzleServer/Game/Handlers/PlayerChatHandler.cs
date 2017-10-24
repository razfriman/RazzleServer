using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using MapleLib.PacketLib;
using RazzleServer.Game;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.GENERAL_CHAT)]
    public class PlayerChatHandler : GamePacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            string message = packet.ReadString();
            byte show = packet.ReadByte();
            var pw = PlayerChatPacket(client.Account.Character.ID, message, show, client.Account.IsGM);
            client.Account.Character.Map.BroadcastPacket(pw);
        }

        public static PacketWriter PlayerChatPacket(int characterId, string message, byte show, bool whiteBackground)
        {
            
            var pw = new PacketWriter(ServerOperationCode.PLAYER_CHAT);
            pw.WriteInt(characterId);
            pw.WriteBool(whiteBackground);
            pw.WriteMapleString(message);
            pw.WriteByte(show);
            pw.WriteBool(false);//isWorldMessage
            pw.WriteByte(0xFF);//if isWorldMessage, this is worldID

            return pw;
        }
    }
}