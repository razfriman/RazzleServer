using System.Diagnostics;
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

            if (Styles != null)
            {
                Styles.Skins.Clear();
                Styles.MaleHairs.Clear();
                Styles.MaleFaces.Clear();
                Styles.FemaleHairs.Clear();
                Styles.FemaleFaces.Clear();
            }

            if (Items != null)
            {
                Items.Clear();
            }

            if (Skills != null)
            {
                Skills.Clear();
            }

            if (Mobs != null)
            {
                Mobs.Clear();
            }

            if (Maps != null)
            {
                Maps.Clear();
            }

            if (Quests != null)
            {
                Quests.Clear();
            }

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

            CommandFactory.Initialize();

            sw.Stop();

            Log.LogInformation("Maple data loaded in {0}ms.", sw.ElapsedMilliseconds);

            IsInitialized = true;
        }
    }
}
