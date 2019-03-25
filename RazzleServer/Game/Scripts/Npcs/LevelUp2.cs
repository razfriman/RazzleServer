﻿using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("levelUP2")]
    public class LevelUp2 : ANpcScript
    {
        public override void Execute()
        {
            SendOk("Welcome to RazzleServer.");
            SendNext("Page 2");
            SendBackNext("Page 3");
            SendBackNext("Page 4");
        }
    }
}
