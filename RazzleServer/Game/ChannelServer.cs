﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using RazzleServer.Server;
using RazzleServer.Util;

namespace RazzleServer.Game
{
    public class WvsGame : MapleServer<GameClient>
    {
        public CenterServer CenterConnection { get; set; }

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

        public WvsGame(ushort port)
        {
            DataProvider.Initialize();

            new Thread(new ThreadStart(CenterServer.Main)).Start();

            WvsGame.CenterConnectionDone.WaitOne();

            byte[] channelIp = { 0, 0, 0, 0 };
            Start(new IPAddress(channelIp), port);
        }

        public override void Dispose()
        {
            CenterConnection?.Dispose();
            ShutDown();
            Log.LogInformation($"Server disposed from thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
