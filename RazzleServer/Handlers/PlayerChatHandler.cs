using Microsoft.Extensions.Logging;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using RazzleServer.Packet;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.GENERAL_CHAT)]
    public class PlayerChatHandler : APacketHandler
    {
        private static ILogger Log = LogManager.Log;

        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            string message = packet.ReadMapleString();
            byte show = packet.ReadByte();

            Log.LogInformation($"{client.Account.Character.Name}: {message}");

            //if (message[0] == '@')
            //{
            //    if (PlayerCommands.ProcessCommand(message.Substring(1).Split(' '), client))
            //        return;
            //}
            //else if (message[0] == '!')
            //{
            //    if (client.Account.IsGM)
            //    {
            //        string[] split = message.Substring(1).Split(' ');
            //        if (GMCommands.ProcessCommand(split, client))
            //            return;
            //        if (client.Account.IsAdmin)
            //        {
            //            if (AdminCommands.ProcessCommand(split, client))
            //                return;
            //            else
            //            {
            //                client.Account.Character.SendBlueMessage("Unrecognized Admin command");
            //                return;
            //            }
            //        }
            //        else
            //        {
            //            client.Account.Character.SendBlueMessage("Unrecognized GM command");
            //            return;
            //        }
            //    }
            //}

            var pw = PlayerChatPacket(client.Account.Character.ID, message, show, client.Account.IsGM);
            client.Account.Character.Map.BroadcastPacket(pw);
        }

        public static PacketWriter PlayerChatPacket(int characterId, string message, byte show, bool whiteBackground)
        {
            
            var pw = new PacketWriter(SMSGHeader.PLAYER_CHAT);
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