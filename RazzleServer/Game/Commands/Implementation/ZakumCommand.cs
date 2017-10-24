using RazzleServer.Player;

namespace RazzleServer.Commands.Implementation
{
    public sealed class ZakumCommand : Command
    {
        public override string Name => "zakum";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(MapleCharacter caller, string[] args)
        {
            if (args.Length != 0)
            {
                ShowSyntax(caller);
                return;
            }

            //caller.Map.Mobs.Add(new Mob(8800000, caller.Position));

            for (int i = 0; i < 7; i++)
            {
                //caller.Map.Mobs.Add(new Mob(8800003 + i, caller.Position));
            }
        }
    }
}
