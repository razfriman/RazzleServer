﻿using System.Linq;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class ClearDropsCommand : ACommandScript
    {
        public override string Name => "cleardrops";

        public override string Parameters => "[ -pickup ]";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            if (args.Length > 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                var pickUp = false;

                if (args.Length == 1)
                {
                    if (args[0].ToLower() == "-pickup")
                    {
                        pickUp = true;
                    }
                    else
                    {
                        ShowSyntax(caller);
                        return;
                    }
                }

                lock (caller.Map.Drops)
                {
                    var toPick = caller.Map.Drops.Values.ToList();

                    foreach (var loopDrop in toPick)
                    {
                        if (pickUp)
                        {
                            caller.Items.Pickup(caller.Map.Drops[loopDrop.ObjectId]);
                        }
                        else
                        {
                            caller.Map.Drops.Remove(loopDrop);
                        }
                    }
                }
            }
        }
    }
}
