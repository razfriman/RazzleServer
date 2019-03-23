# RazzleServer

RazzleServer is a C# server emulator for MapleStory v62.

## Requirements
- MapleStory V62
- [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)


## Running

### Via [Docker](https://www.docker.com/)
```
docker-compose up
```

### Via [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/)
```
cd RazzleServer
dotnet run
```

## Configuration

There is a default configuration provided via `appsettings.json`. 
This contains both the server configuration and the logging configuration.

### Server

| Option        | Description   | Default Value |
| ------------- | ------------- | ------------- |
| LoginPort | The port for the login server | 8484 |
| ChannelPort | The starting port for the channel server. Each channel will increment this as the channels are created| 7575 |
| DefaultMapId  | The map that new characters will start on once they are created | 180000000 |
| DatabaseConnection | Database connection string, this will change based on the database connection type | MapleServer.db |
| DatabaseConnectionType | Which database provider to use. Current supported values or `InMemory` and `Sqlite` | `Sqlite`
...

TODO: List out all of the configuration options...

```
        public string CacheFolder { get; set; } = "DataCache";
        public bool PrettifyCache { get; set; } = false;
        public string WzFilePath { get; set; } = string.Empty;
        public ushort Version { get; set; } = 62;
        public byte SubVersion { get; set; } = 1;
        public byte ServerType { get; set; } = 8;
        public int PingTimeout { get; set; } = 30;
        public bool PrintPackets { get; set; } = true;
        public ulong AesKey { get; set; } = 0x52330F1BB4060813;
        public bool EnableAutoRegister { get; set; } = true;
        public bool EnableMultiLeveling { get; set; } = true;
        public string CommandIndicator { get; set; } = "!";
        public int DefaultCreationSlots { get; set; } = 3;
        public bool RequestPin { get; set; } = true;
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
```

### Logging


Logging is currently configured via [Serilog](https://serilog.net/) with a default configuration already setup in `appsettings.json`. 
However, these are highly customizable using the vast features that Serilog provides

By default, Logs are created in the `Logs` folder:

- `RazzleServer` specific logs are sent to their own log file.
- `Microsoft` specific logs are sent to their own log file. This is helpful useful to see what database queries are being executed.

### Database 

Razzle Server is currently configured to use SQLite via [EF Core (Entity Framework Core)](https://docs.microsoft.com/en-us/ef/core/).
The database will automatically be created at run-time. There is no setup required.

Further, RazzleServer is designed such that it can be extended to support any other [providers supported by EF Core](https://docs.microsoft.com/en-us/ef/core/providers/)
