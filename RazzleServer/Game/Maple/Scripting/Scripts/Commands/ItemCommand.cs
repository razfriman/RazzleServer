using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Scripts.Command{
    public sealed class ItemCommand : ACommandScript
    {
        public override string Name => "item";

        public override string Parameters => "{ id } [ quantity ]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
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
                    short.TryParse(args[args.Length - 1], out quantity);
                }

                if (quantity < 1)
                {
                    quantity = 1;
                }

                var itemId = int.Parse(args[0]);

                if (DataProvider.Items.Data.ContainsKey(itemId))
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
