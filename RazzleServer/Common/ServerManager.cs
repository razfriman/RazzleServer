using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Hosting;
using RazzleServer.Common.Server;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Login;
using Serilog;

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
            await ShopProvider.Initialize();
        }
    }
}
