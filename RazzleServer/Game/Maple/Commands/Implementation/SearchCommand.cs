using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using System.Linq;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class SearchCommand : Command
    {
        public override string Name => "search";

        public override string Parameters => "[ -item | -map | -mob | -npc | -quest ] label";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 2)
            {
                ShowSyntax(caller);
                return;
            }
            var type = args[0];
            var query = CombineArgs(args, 1).ToLower();

            if (query.Length < 2)
            {
                caller.Notify("[Command] Please enter at least 2 characters.");
                return;
            }

            if (type == "-item")
            {
                //SearchItems(args[1]);
            }
            else if (type == "-map")
            {
                //SearchMaps(args[1]);
            }
            else if (type == "-mob")
            {
                //SearchMobs(args[1]);
            }
            else if (type == "-npc")
            {
                //SearchMobs(args[1]);
            }
            else if (type == "-quest")
            {
                //Search(args[1]);
            }

            const bool hasResult = false;

            if (hasResult)
            {
                caller.Notify("[Results]");

            }
            else
            {
                caller.Notify("No result found.");

            }
        }
    }
}
