using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Commands;
using RazzleServer.Game.Maple.Data.Cache;

namespace RazzleServer.Game.Maple.Data
{
    public static class DataProvider
    {
        public static bool IsInitialized { get; private set; }

        public static CachedAvailableStyles Styles { get; private set; }
        public static CachedItems Items { get; private set; }
        public static CachedSkills Skills { get; private set; }
        public static CachedMobs Mobs { get; private set; }
        public static CachedReactors Reactors { get; private set; }
        public static CachedQuests Quests { get; private set; }
        public static CachedCreationData CreationData { get; private set; }
        public static CachedMaps Maps { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static void Initialize()
        {
            IsInitialized = false;

            var sw = new Stopwatch();

            sw.Start();

            Log.LogInformation("Loading data...");

            Styles = new CachedAvailableStyles();
            Items = new CachedItems();
            Skills = new CachedSkills();
            Mobs = new CachedMobs();
            Reactors = new CachedReactors();
            Quests = new CachedQuests();
            CreationData = new CachedCreationData();
            Maps = new CachedMaps();

            Task.WaitAll(
                Task.Run(async () => Styles = await new AvailableStylesDataLoader().Load()),
                Task.Run(async () => Items = await new ItemsLoader().Load()),
                Task.Run(async () => CreationData = await new CreationDataLoader().Load()),
                Task.Run(async () => Skills = await new SkillsLoader().Load()),
                Task.Run(async () => Mobs = await new MobsLoader().Load()),
                Task.Run(async () => Reactors = await new ReactorsLoader().Load()),
                Task.Run(async () => Quests = await new QuestsLoader().Load()),
                Task.Run(async () => Maps = await new MapsLoader().Load()),
                Task.Run(() => CommandFactory.Initialize())
            );

            sw.Stop();

            Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);

            IsInitialized = true;
        }
    }
}
