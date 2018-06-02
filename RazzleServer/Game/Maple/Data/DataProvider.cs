using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.Loaders;

namespace RazzleServer.Game.Maple.Data
{
    public static class DataProvider
    {
        public static CachedAvailableStyles Styles { get; private set; }
        public static CachedItems Items { get; private set; }
        public static CachedSkills Skills { get; private set; }
        public static CachedMobs Mobs { get; private set; }
        public static CachedMobSkills MobSkills { get; private set; }
        public static CachedReactors Reactors { get; private set; }
        public static CachedQuests Quests { get; private set; }
        public static CachedNpcs Npcs { get; private set; }
        public static CachedCreationData CreationData { get; private set; }
        public static CachedMaps Maps { get; private set; }
        public static CachedStrings Strings { get; private set; }
        public static CachedShops Shops { get; private set; }
        public static CachedRechargeTiers RechargeTiers { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static async Task Initialize()
        {
            var sw = Stopwatch.StartNew();

            await Task.WhenAll(
                Task.Run(async () => Styles = await new AvailableStylesDataLoader().Load()),
                Task.Run(async () => Items = await new ItemsLoader().Load()),
                Task.Run(async () => CreationData = await new CreationDataLoader().Load()),
                Task.Run(async () => Skills = await new SkillsLoader().Load()),
                Task.Run(async () => Mobs = await new MobsLoader().Load()),
                Task.Run(async () => Npcs = await new NpcsLoader().Load()),
                Task.Run(async () => MobSkills = await new MobSkillsLoader().Load()),
                Task.Run(async () => Reactors = await new ReactorsLoader().Load()),
                Task.Run(async () => Quests = await new QuestsLoader().Load()),
                Task.Run(async () => Maps = await new MapsLoader().Load()),
                Task.Run(async () => Strings = await new StringLoader().Load())
            );

            sw.Stop();

            Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
        }
    }
}
