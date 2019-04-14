using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class TickerCommand : ACommandScript
    {
        public override string Name => "ticker";

        public override string Parameters => "[ message ]";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            caller.Client.Server.World.TickerMessage = args.Fuse();
            caller.Client.Server.World.UpdateTicker();
        }
    }
}
