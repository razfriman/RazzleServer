using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;

namespace RazzleServer.Center
{
    public class ServerConfig
    {
        public ushort LoginPort { get; set; } = 8484;
        public ushort ChannelPort { get; set; } = 8585;
        public int DefaultMapId { get; set; } = 180000000;
        public string DatabaseName { get; set; } = "MapleServer.db";
        public string CacheFolder { get; set; } = "DataCache";
        public string WzFilePath { get; set; } = string.Empty;
        public ushort Version { get; set; } = 55;
        public byte SubVersion { get; set; } = 1;
        public byte ServerType { get; set; } = 8;
        public int PingTimeout { get; set; } = 30;
        public bool PrintPackets { get; set; } = true;
        public ulong AesKey { get; set; } = 0x52330F1BB4060813;
        public bool EnableAutoRegister { get; set; } = true;
        public bool EnableMultiLeveling { get; set; } = true;
        public string CommandIndicator { get; set; } = "!";
        public int DefaultCreationSlots { get; set; } = 3;
        public bool RequestPin { get; set; }
        public List<WorldConfig> Worlds { get; set; }

        private static readonly ILogger Log = LogManager.Log;

        public static async Task LoadFromFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var contents = await File.ReadAllTextAsync(path);
                    _instance = JsonConvert.DeserializeObject<ServerConfig>(contents);
                }
                else
                {
                    Log.LogWarning($"Config file does not exist at '{path}'.");
                    _instance = GetDefaultConfig();
                    await _instance.SaveToFile(path);
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error deserializing ServerConfig");
            }
        }

        public static ServerConfig GetDefaultConfig()
        {
            return new ServerConfig
            {
                Worlds = new List<WorldConfig>
                {
                    new WorldConfig
                    {
                        Id = 0,
                        Name = WorldName.Scania.ToString(),
                        Channels = 3,
                        Flag = WorldStatusFlag.None,
                        EventMessage = "",
                        TickerMessage = "",
                        EnableCharacterCreation = true,
                        ExperienceRate = 1,
                        QuestExperienceRate = 1,
                        PartyQuestExperienceRate = 1,
                        MesoRate = 1,
                        DropRate = 1
                    }
                }
            };
        }

        private async Task SaveToFile(string path)
        {
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(Instance, Formatting.Indented));
            Log.LogInformation($"Saving default config file to '{path}'.");
        }

        private static ServerConfig _instance;
        public static ServerConfig Instance => _instance ?? (_instance = new ServerConfig());
    }
}