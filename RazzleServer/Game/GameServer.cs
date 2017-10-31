using System.Net;
using Microsoft.Extensions.Logging;
using RazzleServer.Server;
using RazzleServer.Common.Util;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Server;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient>
    {
        public GameCenterClient CenterConnection { get; set; }
        public byte ChannelID { get; set; }
        public WorldConfig World { get; set; }

        public static GameServer CurrentInstance { get; private set; }
        private static readonly ILogger Log = LogManager.Log;

        public GameServer(WorldConfig world)
        {
            World = world;
            Port = world.Port;
            StartCenterConnection(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
            CurrentInstance = this;
        }

        public override void ServerRegistered()
        {
            Log.LogInformation($"Registered Game Server ({World.Name} [{World.ID}]-{ChannelID}).");
            Start(IPAddress.Loopback, Port);
        }

        public override void CenterServerConnected()
        {
            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Channel);
            CenterConnection.Send(pw);
        }

        public override void Dispose()
        {
            CenterConnection?.Dispose();
            ShutDown();
        }
    }
}
