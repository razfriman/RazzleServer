using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Server;
using RazzleServer.Common.Util;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Constants;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient>
    {
        public GameCenterClient CenterConnection { get; set; }

        public byte ChannelID { get; set; }
        public byte WorldID { get; set; }
        public string WorldName { get; set; }
        public string TickerMessage { get; set; }
        public int ExperienceRate { get; set; }
        public int QuestExperienceRate { get; set; }
        public int PartyQuestExperienceRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }

        private static readonly ILogger Log = LogManager.Log;

        public GameServer(ushort port)
        {
            Port = port;
            StartCenterConnection(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
        }

        public override void ServerRegistered()
        {
            Log.LogInformation($"Registered Game Server ({WorldName} [{WorldID}]-{ChannelID}).");
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
