namespace RazzleServer.Server
{
    public class ServerConfig
    {
        public int ExpRate { get; set; }
        public int MesoRate { get; set; }
        public int DropRate { get; set; }
        public int QuestExpRate { get; set; }
        public ushort LoginPort { get; set; } = 8484;
        public ushort ChannelStartPort { get; set; } = 8585;
        public byte Channels { get; set; } = 1;
        public string WorldName{get;set;} = "Scania";
        public byte WorldFlag {get;set;} = 2;
        public string EventMessage {get;set; } = "Raz's Event";
        public int DefaultMapID{get;set;} = 140090000;
        public string DatabaseName { get; set; } = "MapleServer.db";
        public ushort Version { get; set; } = 83;
        public byte SubVersion { get; set; } = 1;
        public byte ServerType { get; set; } = 8;
        public long CipherKey { get; set; } = 0x00001;
        public int PingTimeout { get; set; } = 30;
        public bool PrintPackets { get; set; } = true;
        public bool IsLocalHost { get; set; } = true;
        public ulong AESKey { get; set; } = 0x52330F1BB4060813;
        public bool LoginCreatesNewAccount { get; set; } = true;


        private static ServerConfig _instance = null;
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