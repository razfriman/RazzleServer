using System.Threading.Tasks;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Common
{
    public class ServerManager : AServerManager
    {
        public override async Task Configure()
        {
            await base.Configure();
            await CachedData.Initialize();
            await ScriptProvider.Initialize();
            await LootProvider.Initialize();
        }
    }
}
