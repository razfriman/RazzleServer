using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class SayCommand : ACommandScript
    {
        public override string Name => "say";

        public override string Parameters => "message";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                var message = args.Fuse();
                caller.Client.Server.World.Send(GamePackets.Notify(message));
            }
        }
    }
}
