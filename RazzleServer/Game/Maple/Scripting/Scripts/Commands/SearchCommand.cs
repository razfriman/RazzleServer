﻿using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Scripting.Scripts.Commands
{
    public sealed class SearchCommand : ACommandScript
    {
        public override string Name => "search";

        public override string Parameters => "[ -item | -map | -mob | -npc | -quest | -pet ] label";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
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

            if (type == "item")
            {
                lookup = DataProvider.Strings.Items;
            }
            else if (type == "map")
            {
                lookup = DataProvider.Strings.Maps;

            }
            else if (type == "mob")
            {
                lookup = DataProvider.Strings.Mobs;

            }
            else if (type == "npc")
            {
                lookup = DataProvider.Strings.Npcs;
            }
            else if (type == "pet")
            {
                lookup = DataProvider.Strings.Pets;
            }
            else if (type == "skill")
            {
                lookup = DataProvider.Strings.Skills;
            }
            else if (type == "quest")
            {
                lookup = DataProvider.Quests.Data.Values.ToDictionary(x => (int)x.MapleId, x => x.Name);
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