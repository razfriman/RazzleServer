using System.Linq;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class KillMobsCommand : Command
    {
        public override string Name => "killmobs";

        public override string Parameters => "[ - drop ]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length > 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                var drop = false;

                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "-drop" || args[0].ToLower() == "-drops")
                    {
                        drop = true;
                    }
                    else
                    {
                        ShowSyntax(caller);

                        return;
                    }
                }

                lock (caller.Map.Mobs)
                {
                    var toKill = caller.Map.Mobs.Values.ToList();

                    foreach (var loopMob in toKill)
                    {
                        loopMob.CanDrop = drop;
                        loopMob.Die();
                    }
                }
            }
        }
    }
}
