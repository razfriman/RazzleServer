using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripting.Scripts.Commands
{
    public sealed class TickerCommand : ACommandScript
    {
        public override string Name => "ticker";

        public override string Parameters => "[ message ]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            var message = args.Length > 0 ? args[0] : string.Empty;
            caller.Client.Server.World.TickerMessage = message;
            caller.Client.Server.World.Send(GamePackets.Notify(message, NoticeType.ScrollingText));
        }
    }
}
