using System;
using System.Diagnostics;
using System.Runtime;
using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.Loaders;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider
{
    public class CachedData
    {
        public static WzFile WzFile = null;
        public static CachedAvailableStyles Styles { get; private set; } = new CachedAvailableStyles();
        public static CachedItems Items { get; private set; } = new CachedItems();
        public static CachedSkills Skills { get; private set; } = new CachedSkills();
        public static CachedMobs Mobs { get; private set; } = new CachedMobs();
        public static CachedMobSkills MobSkills { get; private set; } = new CachedMobSkills();
        public static CachedNpcs Npcs { get; private set; } = new CachedNpcs();
        public static CachedCreationData CreationData { get; private set; } = new CachedCreationData();
        public static CachedMaps Maps { get; private set; } = new CachedMaps();
        public static CachedStrings Strings { get; private set; } = new CachedStrings();
        public static CachedRechargeTiers RechargeTiers { get; } = new CachedRechargeTiers();
        public static CachedQuests Quests { get; private set; } = new CachedQuests();
        public static CachedQuizzes Quizzes { get; private set; } = new CachedQuizzes();
        public static CachedBuffs Buffs { get; } = new CachedBuffs();

        private static readonly ILogger Logger = Log.ForContext<CachedData>();

        public static void Initialize()
        {
            var sw = Stopwatch.StartNew();
            Styles = new AvailableStylesDataLoader().Load();
            CreationData = new CreationDataLoader().Load();
            Skills =  new SkillsLoader().Load();
            Mobs =  new MobsLoader().Load();
            Npcs =  new NpcsLoader().Load();
            MobSkills =  new MobSkillsLoader().Load();
            Strings =  new StringLoader().Load();
            Quests =  new QuestsLoader().Load();
            Quizzes =  new QuizzesLoader().Load();
            Maps =  new MapsLoader().Load();
            Items = new ItemsLoader().Load();
            WzFile?.Dispose();

            sw.Stop();

            Logger.Information("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
        }
    }
}
