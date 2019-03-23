using System;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("secretNPC")]
    public class SecretNpc : ANpcScript
        {
            public override void Execute() => throw new NotImplementedException();
        }
    }