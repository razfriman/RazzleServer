using System;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("multipet_fail")]
    public class MultipetFail : ANpcScript
        {
            public override void Execute() => throw new NotImplementedException();
        }
    }