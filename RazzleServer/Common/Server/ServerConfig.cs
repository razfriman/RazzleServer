using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using System.Threading.Tasks;
using System.Collections.Generic;
using RazzleServer.Common.Server;

namespace RazzleServer.Server
{
	public class ServerConfig
	{
		public ushort LoginPort { get; set; } = 8484;
		public int DefaultMapID { get; set; } = 100000000;
		public string DatabaseName { get; set; } = "MapleServer.db";
        public string WzFilePath { get; set; } = string.Empty;
		public ushort Version { get; set; } = 83;
		public byte SubVersion { get; set; } = 1;
		public byte ServerType { get; set; } = 8;
		public int PingTimeout { get; set; } = 30;
		public bool PrintPackets { get; set; } = true;
		public ulong AESKey { get; set; } = 0x52330F1BB4060813;
		public bool LoginCreatesNewAccount { get; set; } = true;
        public string CommandIndicator { get; set; } = "!";
        public ushort CenterPort { get; set; } = 8181;
        public IEnumerable<WorldConfig> Worlds { get; set; }

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