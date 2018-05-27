using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class SearchCommand : Command
    {
        public override string Name => "search";

        public override string Parameters => "[ -item | -map | -mob | -npc | -quest ] label";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length == 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                var query = args[0].StartsWith("-") ? CombineArgs(args, 1).ToLower() : CombineArgs(args).ToLower();

                if (query.Length < 2)
                {
                    caller.Notify("[Command] Please enter at least 2 characters.");
                }
                else
                {
                    const bool hasResult = false;

                    caller.Notify("[Results]");

                    //foreach (Datum datum in new Datums("strings").Populate("`label` LIKE '%{0}%'", query))
                    //{
                    //    caller.Notify(string.Format("   -{0}: {1}", (string)datum["label"], (int)datum["objectid"]));

                    //    hasResult = true;
                    //}

                    if (!hasResult)
                    {
                        caller.Notify("   No result found.");
                    }
                }
            }
        }
    }
}
