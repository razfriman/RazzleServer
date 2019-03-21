using System;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public class MapCommand : ACommandScript
    {
        public override string Name => "map";

        public override string Parameters => "{ { id | keyword | exact name } [portal] | -current }";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length == 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                if (args.Length == 1 && args[0] == "-current")
                {
                    caller.Notify("[Command] Current map: " + caller.Map.MapleId);
                    caller.Notify("   -X: " + caller.Position.X);
                    caller.Notify("   -Y: " + caller.Position.Y);
                }
                else
                {
                    var mapName = "";
                    int mapId = int.TryParse(args[0], out mapId) ? mapId : -1;
                    byte portalId = 0;

                    if (args.Length >= 2)
                    {
                        byte.TryParse(args[1], out portalId);
                    }

                    if (mapId == -1)
                    {
                        mapName = string.Join(" ", args);
                        Enum.TryParse(mapName, true, out CommandMaps val);
                        if (val > 0)
                        {
                            mapId = (int)val;
                        }
                    }

                    if (DataProvider.Maps.Data.ContainsKey(mapId))
                    {
                        caller.ChangeMap(mapId, portalId);
                    }
                    else
                    {
                        caller.Notify($"[Command] Invalid map name: {mapName}");
                    }
                }
            }
        }

        private enum CommandMaps
        {
            MushroomTown = 10000,
            Amherst = 1000000,
            Southperry = 2000000,
            Henesys = 100000000,
            SomeoneElsesHouse = 100000005,
            HenesysMarket = 100000100,
            HenesysPark = 100000200,
            HenesysGamePark = 100000203,
            Ellinia = 101000000,
            MagicLibrary = 101000003,
            ElliniaStation = 101000300,
            Perion = 102000000,
            KerningCity = 103000000,
            SubwayTicketingBooth = 103000100,
            KerningSquare = 103040000,
            LithHarbor = 104000000,
            ThicketAroundtheBeachIii = 104000400,
            ThePigBeach = 104010001,
            Sleepywood = 105040300,
            RegularSauna = 105040401,
            VipSauna = 105040402,
            AntTunnel = 105050000,
            AntTunnelPark = 105070001,
            TheGraveofMushmom = 105070002,
            OxQuiz = 109020001,
            OlaOla = 109030001,
            MapleStoryPhysicalFitnessTest = 109040000,
            Snowball = 109060000,
            MinigameChallenge = 109070000,
            CoconutHarvest = 109080000,
            FlorinaBeach = 110000000,
            Ereve = 130000000,
            Rien = 140000000,
            Gm = 180000000,
            Blank = 180000001,
            Orbis = 200000000
        }
    }
}
