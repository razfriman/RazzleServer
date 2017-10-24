using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Util;
using System.Threading.Tasks;

namespace RazzleServer.Server
{
	public class ServerConfig
	{
        public int ExpRate { get; set; } = 1;
        public int MesoRate { get; set; } = 1;
        public int DropRate { get; set; } = 1;
        public int QuestExpRate { get; set; } = 1;
		public ushort LoginPort { get; set; } = 8484;
		public ushort ChannelStartPort { get; set; } = 8585;
		public byte Channels { get; set; } = 1;
        public byte Worlds { get; set; } = 1;
		public string EventMessage { get; set; } = "Raz's Event";
		public int DefaultMapID { get; set; } = 100000000;
		public string DatabaseName { get; set; } = "MapleServer.db";
        public string WzFilePath { get; set; } = string.Empty;
		public ushort Version { get; set; } = 83;
		public byte SubVersion { get; set; } = 1;
		public byte ServerType { get; set; } = 8;
		public int PingTimeout { get; set; } = 30;
		public bool PrintPackets { get; set; } = true;
		public bool IsLocalHost { get; set; } = true;
		public ulong AESKey { get; set; } = 0x52330F1BB4060813;
		public bool LoginCreatesNewAccount { get; set; } = true;
        public string CommandIndiciator { get; set; } = "!";
        public ulong CenterServerKey { get; set; } = 0x5233EF1BD4160412;
        public ushort CenterPort { get; set; } = 8181;

        private static ILogger Log = LogManager.Log;

		public static async Task LoadFromFile(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					string contents = await File.ReadAllTextAsync(path);
					_instance = JsonConvert.DeserializeObject<ServerConfig>(contents);
                } else {
                    Log.LogWarning($"Using default config. Config file does not exist at '{path}'");
                }
			}
			catch (Exception e)
			{
				Log.LogError(e, "Error deserializing ServerConfig");
			}
		}

		private static ServerConfig _instance;
		public static ServerConfig Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ServerConfig();
				}
				return _instance;
			}
		}

	}
}