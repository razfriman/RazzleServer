using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class ItemCommand : ACommandScript
    {
        public override string Name => "item";

        public override string Parameters => "{ id } [ quantity ]";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            if (args.Length < 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                short quantity = 0;

                if (args.Length > 1)
                {
                    short.TryParse(args[^1], out quantity);
                }

                if (quantity < 1)
                {
                    quantity = 1;
                }

                var itemId = int.Parse(args[0]);

                if (CachedData.Items.Data.ContainsKey(itemId))
                {
                    caller.Items.Add(new Item(itemId, quantity));
                }
                else
                {
                    caller.Notify("[Command] Invalid item.");
                }
            }
        }
    }
}
