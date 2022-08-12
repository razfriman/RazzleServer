# Summary

- MapleStory v40b 
- C# .NET
- Cross platform - Windows, Mac, Linux

## Configuration 

- Zero configuration required for startup
- Rich configuration via a single `appsettings.json` file
- Settings documented on `README.md`
- Fast start up time (less than `3s`)
- Server data generated from WZ
- Configurable logging via `Serilog`
- Server data cached as `JSON` for easy readability
- Unit test support for all packet and crypto operations
- Low memory usage (56MB on startup)
- Database-agnostic via EF-Core

## Deployment

- CI pipeline via Azure Pipelines
- Docker support for easy deployment

# Features

## Login

The login server is fully implmented

## Channel

### NPCs

- Scripts compiled for seamless developement experience

Simple Example
```C#
[NpcScript("levelUP2")]
public class LevelUp2 : ANpcScript
{
    public override void Execute() => SendOk("Welcome to RazzleServer");
}
```
Advanced Example
```C#
[NpcScript("levelUP")]
public class LevelUp : ANpcScript
{
    public override void Execute()
    {
        SendNext("Welcome to RazzleServer");
        var mapIds = new[] { 100000000, 101000000 };
        var result = SendChoice("Where do you want to go?" + CreateSelectionList(NpcListType.Map, mapIds));
        if (result >= 0)
        {
            Character.ChangeMap(mapIds[result]);
        }
    }
}
```

### Items

- Megaphones
- AP Reset
- Weather effect items
- Jukebox
- Meso sacks
- Return scrolls
- Summon bags
- Teleport Rocks

### Mobs

- Functionin
- Drop data initialization stored as JSON

### Etc.

- Storage
- Admin commands

# Advanced

## Packet / Network IO

Using `System.IO.Pipelines` for fast, efficient, low-allocation network IO.

Configurable packet logging + easily ignore common packets via the `IgnorePacketPrint` attribute:
```C#
[IgnorePacketPrint] Pong = 0x09,
```

Easily configure packet handlers to `ClientOperationCode` by wiring them up via the `PacketHandler` attribute:

```C#
[PacketHandler(ClientOperationCode.Pong)]
public class PongHandler : GamePacketHandler
{
    public override void HandlePacket(PacketReader packet, GameClient client) => client.LastPong = DateTime.UtcNow;
}
```

## Logging

Sample logs with rich context to assis with debugging packets 

Example:
```json
{"@t":"2019-04-01","@mt":"Starting LoginServer on port [8484]","SourceContext":"RazzleServer.Login.LoginServer"}
{"@t":"2019-04-01","@mt":"Starting GameServer on port [7575]","SourceContext":"RazzleServer.Game.GameServer"}
{"@t":"2019-04-01","@mt":"Client Connected","SourceContext":"RazzleServer.Login.LoginServer"}
{"@t":"2019-04-01","@mt":"Received [Login] 01 05 00 00 00 FF 3D 00 00 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [CheckPasswordResult] 01 00 05 00 61 64 6D 69 6E","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [WorldInformation] 03 00 06 00 54 00 00 00 00 02 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [WorldInformation] 03 FF 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Received [WorldStatus] 03 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [WorldStatus] 02 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Received [SelectChannel] 02 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [CharacterList] 04 00 01 02 00 00 00 00 00 00 00 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Received [SelectCharacter] 04 02 00 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Sending [ClientConnectToServerLogin] 05 00 1D 02 00 00 00 00","SourceContext":"RazzleServer.Login.LoginClient"}
{"@t":"2019-04-01","@mt":"Client Connected","SourceContext":"RazzleServer.Game.GameServer"}
{"@t":"2019-04-01","@mt":"Received [CharacterLoad] 05 02 00 00 00","SourceContext":"RazzleServer.Game.GameClient"}

```