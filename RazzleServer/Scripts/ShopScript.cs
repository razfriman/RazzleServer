using RazzleServer.Map;
using System.Collections.Generic;

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