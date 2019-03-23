# RazzleServer

RazzleServer is a C# server emulator for MapleStory v62.

## Requirements
- MapleStory V62
- .NET Core 3.0+


## Running

### Via Docker
```
docker-compose up
```

### Via CLI
```
cd RazzleServer
dotnet run
```

### Setup Guide
1. **Get Destiny**: Clone the Git repository to a local folder on your machine.
2. **Download and install Visual Studio**: The community version is offered for free on Microsoft website.
3. **Download and install MySQL**: Install a SQL server of your choice (pref. WampServer).
4. **Check LIB references for project**: Check that MoonSharp and MySQL .net DB connector are referenced rightly, if not use NutGet manager to install packages. Right click references -> manage NuGet packages, MySql.Data version 6.10.6.0 and MoonSharp.Interpreter version 2.0.0.0.  
4. **Build the Destiny solution provided with Visual Studio**
5. **Run Destiny**: Execute the servers in order: WvsCenter -> WvsLogin -> WvsGame(s).

Each server will guide you through the process of configuring it automatically.

**Warning: Two auto-setup options currently dont function as intented.**
- If login for first time with autoregister function set you will have to login for 2 times consequtively. On the fist time you wont be recognized and thefore on second attemp you will be autoregisterted and loged in.

- The number of channels selected on login server setup is not yet implemented. So even if you select 16 channels there will be only 1


## Database

Razzle Server is currently configured to use SQLite. The database will automatically be created at run-time. There is no setup required.


### Database Provider

RazzleServer uses [EF Core (Entity Framework Core)](https://docs.microsoft.com/en-us/ef/core/). 

However, it can be extended to support any other [providers supported by EF Core](https://docs.microsoft.com/en-us/ef/core/providers/)

## Logging

Logging is currently configured via Serilog.

Logs are created in the `Logs` folder. 

`RazzleServer` specific logs are sent to their own log file.
`Microsoft` specific logs are also sent to their own log file. This is helpful in logging what database queries are being executed.


This is the default Serilog configuration.
```C#
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.FromSource("Microsoft"))
        .WriteTo.File(new CompactJsonFormatter(),"Logs/Microsoft.log", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.FromSource("Microsoft"))
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.File(new CompactJsonFormatter(), "Logs/RazzleServer.log", rollingInterval: RollingInterval.Day))
    .CreateLogger();
```


## Default Configuration

```json
{
  "LoginPort": 8484,
  "ChannelPort": 7575,
  "DefaultMapId": 180000000,
  "DatabaseConnection": "MapleServer.db",
  "DatabaseConnectionType": "Sqlite",
  "CacheFolder": "/app/Data/DataCache",		
  "WzFilePath": "/app/Data/WZ",
  "PrettifyCache": false,
  "Version": 62,
  "SubVersion": 1,
  "ServerType": 8,
  "PingTimeout": 30,
  "PrintPackets": false,
  "AesKey": 5923094546581162003,
  "EnableAutoRegister": true,
  "EnableMultiLeveling": true,
  "CommandIndicator": "!",
  "DefaultCreationSlots": 3,
  "RequestPin": false,
  "Worlds": [
    {
      "Id": 0,
      "Name": "Scania",
      "Channels": 3,
      "Flag": "None",
      "EventMessage": "",
      "TickerMessage": "",
      "EnableCharacterCreation": true,
      "ExperienceRate": 10,
      "QuestExperienceRate": 10,
      "PartyQuestExperienceRate": 10,
      "MesoRate": 10,
      "DropRate": 10
    }
  ]
}

```