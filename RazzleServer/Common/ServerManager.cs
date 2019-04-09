using System.Threading.Tasks;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Common
{
    public class ServerManager : AServerManager
    {
        public override async Task Configure()
        {
            await base.Configure();
            await DataProvider.Initialize();
            await ScriptProvider.Initialize();
            await LootProvider.Initialize();
        }
    }
}
