using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RazzleServer.Common.Constants;

namespace RazzleServer.Common
{
    public class ServerConfig
    {
        public ushort LoginPort { get; set; } = 8484;
        public ushort ChannelPort { get; set; } = 7575;
        public int DefaultMapId { get; set; } = 180000000;
        public string DatabaseConnection { get; set; } = "MapleServer.db";
        public DatabaseConnectionType DatabaseConnectionType { get; set; } = DatabaseConnectionType.Sqlite;
        public string CacheFolder { get; set; } = "DataCache";
        public bool PrettifyCache { get; set; } = false;
        public string WzFilePath { get; set; } = string.Empty;
        public ushort Version { get; set; } = 40;
        public byte SubVersion { get; set; } = 1;
        public byte ServerType { get; set; } = 5;
        public int PingTimeout { get; set; } = 30;
        public bool PrintPackets { get; set; } = true;
        public ulong AesKey { get; set; } = 0x52330F1BB4060813;
        public bool UseAesEncryption { get; set; } = false;
        public bool EnableAutoRegister { get; set; } = true;
        public bool EnableMultiLeveling { get; set; } = true;
        public string CommandIndicator { get; set; } = "!";
        public int DefaultCreationSlots { get; set; } = 3;
        public List<WorldConfig> Worlds { get; set; }

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

        private static ServerConfig _instance;
        public static ServerConfig Instance => _instance ??= new ServerConfig();

        public static void Load(IConfiguration configuration)
        {
            _instance = GetDefaultConfig();
            configuration.GetSection("RazzleServerConfig").Bind(_instance);
        }
    }
}
