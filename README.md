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

## Configuration

There is a default configuration provided via `appsettings.json`. 
This contains both the server configuration and the logging configuration.

### Server

TODO: List out all of the configuration options...

### Logging


Logging is currently configured via Serilog with a default configuration already setup in `appsettings.json`. 
However, these are highly customizable using the vast features that Serilog provides

By default, Logs are created in the `Logs` folder:

- `RazzleServer` specific logs are sent to their own log file.
- `Microsoft` specific logs are sent to their own log file. This is helpful useful to see what database queries are being executed.

### Database 

Razzle Server is currently configured to use SQLite. The database will automatically be created at run-time. There is no setup required.

RazzleServer uses [EF Core (Entity Framework Core)](https://docs.microsoft.com/en-us/ef/core/). 

However, it can be extended to support any other [providers supported by EF Core](https://docs.microsoft.com/en-us/ef/core/providers/)
