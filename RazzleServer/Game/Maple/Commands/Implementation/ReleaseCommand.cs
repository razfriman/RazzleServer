using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class ReleaseCommand : Command
    {
        public override string Name => "release";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => false;

        public override void Execute(Character caller, string[] args)
        {
            caller.Release();
        }
    }
}
