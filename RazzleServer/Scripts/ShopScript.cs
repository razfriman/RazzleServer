using RazzleServer.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Scripts
{
    public abstract class ShopScript : NpcScript
    {
        public abstract List<ShopItem> ShopItems { get; }

        public override void Execute()
        {

        }

        public override bool IsShop => true;
    }
}