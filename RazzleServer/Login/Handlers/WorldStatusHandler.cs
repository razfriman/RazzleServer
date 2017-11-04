using System;
using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldStatus)]
    public class WorldStatusHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            byte worldID = packet.ReadByte();

            if (client.Server.Manager.Worlds.Contains(worldID))
            {
                var world = client.Server.Manager.Worlds[worldID];
                using (var oPacket = new PacketWriter(ServerOperationCode.CheckUserLimitResult))
                {
                    oPacket.WriteShort((short)world.Flag);
                    client.Send(oPacket);
                }
            }
        }
    }
}