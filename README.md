# RazzleServer

[![Build Status](https://dev.azure.com/Razfriman/razzleserver/_apis/build/status/razzleserver-CI?branchName=master)](https://dev.azure.com/Razfriman/razzleserver/_build/latest?definitionId=2&branchName=master)

RazzleServer is a C# server emulator for MapleStory v40b.

## Requirements

- MapleStory V40b
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

| Option                 | Description                                                                                            | Default Value  |
| ---------------------- | ------------------------------------------------------------------------------------------------------ | -------------- |
| LoginPort              | The port for the login server                                                                          | 8484           |
| ChannelPort            | The starting port for the channel server. Each channel will increment this as the channels are created | 7575           |
| DefaultMapId           | The map that new characters will start on once they are created                                        | 180000000      |
| DatabaseConnection     | Database connection string, this will change based on the database connection type                     | MapleServer.db |
| DatabaseConnectionType | Which database provider to use. Current supported values or `InMemory` and `Sqlite`                    | `Sqlite`       |
| WzFilePath | Path to a directory containing MapleStory .wz files. Only required if no data cache is supplied |  |
| CacheFolder | Path to the RazzleServer data cache files. These are included by default. | DataCache |
| PrettifyCache | Output the cache files in an indented/readable format (uses more space) | false |
| Version | x | 40 |
| SubVersion | x | 1 |
| ServerType | x | 5 |
| PingTimeout | x | 30 |
| PrintPackets | x | true |
| AesKey | x | 0x52330F1BB4060813 |
| EnableAesEncrpytion | Enable MapleStory AES encrpytion, false for v40b | false |
| EnableAutoRegister | Allow account creation by logging in with an unregistered account | true |
| EnableMultiLeveling | Allow gaining multiple levels at once | true |
| CommandIndicator | Prefix character for server commands performed in game chat | '!' |
| Worlds | World configuration | `{` | 
| | | `"Id": 0,`|
| | | `"Name": "Tespia",`|
| | | `"Channels": 3,`|
| | | `"Flag": "None",`|
| | | `"EventMessage": "",`|
| | | `"TickerMessage": "",`|
| | | `"EnableCharacterCreation": true,`|
| | | `"ExperienceRate": 10,`|
| | | `"QuestExperienceRate": 10,`|
| | | `"PartyQuestExperienceRate": 10,`|
| | | `"MesoRate": 10,`|
| | | `"DropRate": 10`|
| | | `}`|

### Logging

Logging is currently configured via [Serilog](https://serilog.net/) with a default configuration already setup in `appsettings.json`.
However, these are highly customizable using the vast features that Serilog provides

By default, Logs are created in the `Logs` folder

### Database

RazzleServer is currently configured to use SQLite via [EF Core (Entity Framework Core)](https://docs.microsoft.com/en-us/ef/core/).
The database will automatically be created at run-time. There is no setup required.

Further, RazzleServer is designed such that it can be extended to support any other [providers supported by EF Core](https://docs.microsoft.com/en-us/ef/core/providers/)
