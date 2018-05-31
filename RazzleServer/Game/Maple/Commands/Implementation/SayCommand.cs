using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class SayCommand : Command
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
                var message = CombineArgs(args);
                caller.Client.Server.World.Send(GamePackets.Notify(message));
            }
        }
    }
}
