# RazzleServer

[![Build Status](https://dev.azure.com/Razfriman/razzleserver/_apis/build/status/razzleserver-CI?branchName=master)](https://dev.azure.com/Razfriman/razzleserver/_build/latest?definitionId=2&branchName=master) 
 [![Docker Pulls](https://img.shields.io/docker/pulls/razfriman/razzleserver.svg)](https://hub.docker.com/r/razfriman/razzleserver)

RazzleServer is a C# server emulator for MapleStory v40b.

## Requirements

- MapleStory v40b
- [.NET Core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)

## Run

### Via [Docker](https://www.docker.com/)

```
docker run razfriman/razzleserver
```

### Via [Docker Compose](https://docs.docker.com/compose/) (Using the included `docker-compose.yml` file)

```
docker-compose up
```

### Build and Run via [.NET CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/)

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
| ShopPort               | The port for the shop server                                                                           | 8787           |
| DefaultMapId           | The map that new characters will start on once they are created                                        | 180000000      |
| DatabaseConnection     | Database connection string, this will change based on the database connection type                     | MapleServer.db |
| DatabaseConnectionType | Which database provider to use. Current supported values or `InMemory` and `Sqlite`                    | `Sqlite`       |
| WzFilePath | Path to a directory containing MapleStory .wz files. Only required if no data cache is supplied |  |
| CacheFolder | Path to the RazzleServer data cache files. These are included by default. | DataCache |
| PrettifyCache | Output the cache files in an indented/readable format (uses more space) | false |
| Version | MapleStory version | 40 |
| SubVersion | MapleStory subversion | 1 |
| ServerType | MapleStory Server Type | 5 |
| PingTimeout | Timeout to disconnect stale client | 30 |
| PrintPackets | Print sent/received packets to the console | true |
| AesKey | Key to use for AES Encryption | 0x52330F1BB4060813 |
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

### Docker Configuration

Use `docker-compose` to easily define volumes where you can configure server settings. This lets you view/change the `appsettings.json` file as well as view the logs. This is also possible via the `Docker CLI` in a more verbose way.

#### Docker Compose Sample:

```yaml
version: "3.7"

services:
  razzleserver:
    image: razfriman/razzleserver:latest
    ports:
      - "8484:8484"
      - "7575-7577:7575-7577"
      - "8787:8787"
    volumes:
      - ./Data/appsettings.json:/app/appsettings.json
      - ./Data/DataCache:/app/Data/DataCache
      - ./Data/WZ:/app/Data/WZ:ro
      - ./Logs:/app/Logs
```
