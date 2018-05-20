using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class ItemCommand : Command
    {
        public override string Name
        {
            get
            {
                return "item";
            }
        }

        public override string Parameters
        {
            get
            {
                return "{ id } [ quantity ]";
            }
        }

        public override bool IsRestricted
        {
            get
            {
                return true;
            }
        }

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 1)
            {
                this.ShowSyntax(caller);
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

                int itemId = int.Parse(args[0]);

                if (DataProvider.Items.Contains(itemId))
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
