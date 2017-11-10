using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Commands;

namespace RazzleServer.Game.Maple.Data
{
    public static class DataProvider
    {
        public static bool IsInitialized { get; private set; }
        public static AvailableStyles Styles { get; private set; }
        public static CachedItems Items { get; private set; }
        public static CachedSkills Skills { get; private set; }
        public static CachedMobs Mobs { get; private set; }
        public static CachedReactors Reactors { get; private set; }
        public static CachedQuests Quests { get; private set; }
        public static CreationData CreationData { get; private set; }
        public static CachedMaps Maps { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static void Initialize()
        {
            IsInitialized = false;

            var sw = new Stopwatch();

            sw.Start();

            Log.LogInformation("Loading data...");

            Styles = new AvailableStyles();
            Items = new CachedItems();
            Skills = new CachedSkills();
            Mobs = new CachedMobs();
            Reactors = new CachedReactors();
            Quests = new CachedQuests();
            CreationData = new CreationData();
            Maps = new CachedMaps();

            Task.WaitAll(
                Task.Run(() => Styles.Load()),
                Task.Run(() => Items.Load()),
                Task.Run(() => Skills.Load()),
                Task.Run(() => Mobs.Load()),
                Task.Run(() => Reactors.Load()),
                Task.Run(() => Quests.Load()),
                Task.Run(() => CreationData.Load()),
                Task.Run(() => Maps.Load()),
                Task.Run(() => CommandFactory.Initialize())
            );

            sw.Stop();

            Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);

            IsInitialized = true;
        }
    }
}
