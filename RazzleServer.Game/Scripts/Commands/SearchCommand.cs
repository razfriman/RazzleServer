using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class SearchCommand : ACommandScript
    {
        public override string Name => "search";

        public override string Parameters => "[ -item | -map | -mob | -npc ] label";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            if (args.Length < 2)
            {
                ShowSyntax(caller);
                return;
            }

            var type = args[0].StartsWith('-') ? args[0].Substring(1) : args[0];
            var query = args.Fuse(1);

            if (query.Length < 2)
            {
                caller.Notify("[Command] Please enter at least 2 characters.");
                return;
            }

            Dictionary<int, string> lookup = null;

            switch (type)
            {
                case "item":
                    lookup = CachedData.Strings.Items;
                    break;
                case "map":
                    lookup = CachedData.Strings.Maps;
                    break;
                case "mob":
                    lookup = CachedData.Strings.Mobs;
                    break;
                case "npc":
                    lookup = CachedData.Strings.Npcs;
                    break;
                case "skill":
                    lookup = CachedData.Strings.Skills;
                    break;
            }

            if (lookup == null)
            {
                caller.Notify($"Invalid search type [{type}]");
                return;
            }

            var results = lookup
                .Where(x => x.Value.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (results.Any())
            {
                caller.Notify($"Results [{type}]");
                results.ForEach(x =>
                {
                    caller.Notify($"[{x.Key}] - {x.Value}");
                });
            }
            else
            {
                caller.Notify("No result found.");
            }
        }
    }
}
