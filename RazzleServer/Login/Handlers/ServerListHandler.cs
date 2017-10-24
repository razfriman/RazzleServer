﻿using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.SERVERLIST_REQUEST)]
    [PacketHandler(ClientOperationCode.SERVERLIST_REREQUEST)]
    public class ServerListHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            //Todo: Loop for each world and channel
            
            var pw = new PacketWriter(ServerOperationCode.SERVERLIST);
            pw.WriteByte(0); // World id
            pw.WriteMapleString(ServerConfig.Instance.WorldName);
            pw.WriteByte(ServerConfig.Instance.WorldFlag);
            pw.WriteMapleString(ServerConfig.Instance.EventMessage);
            pw.WriteByte(0x64);
            pw.WriteByte(0);
            pw.WriteByte(0x64);
            pw.WriteByte(0);
            pw.WriteByte(0);

            var channelCount = ServerConfig.Instance.Channels;
            pw.WriteByte(channelCount);

            for (short i = 0; i < channelCount; i++)
            {
                pw.WriteMapleString($"{ServerConfig.Instance.WorldName}-{i}");
                pw.WriteInt(0); //load
                pw.WriteByte(0); //World id
                pw.WriteShort(i); //channel index
            }
            pw.WriteShort(0);
            client.Send(pw);
            
            pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SERVERLIST);
            pw.WriteByte(0xFF);
            pw.WriteByte(0);
            client.Send(pw);
        }
    }
}