using Serilog;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class QuestsLoader : ACachedDataLoader<CachedQuests>
    {
        public override string CacheName => "Quests";

        public override ILogger Logger => Log.ForContext<QuestsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Quests");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var dir = file.WzDirectory.GetDirectoryByName("Etc");
                var img = dir.GetImageByName("QuestInfo.img");

                foreach (var item in img.WzProperties)
                {
                    if (int.TryParse(item.Name, out var questId))
                    {
                        Data.Data.Add(questId,
                            new QuestReference {MapleId = questId, Name = item["info"]?["subject"]?.GetString()});
                    }
                }
            }
        }
    }
}
