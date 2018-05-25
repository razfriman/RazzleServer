using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class ShopCommand : Command
    {
        public override string Name => "shop";

        public override string Parameters => "[ gear | scrolls | nx | face | ring | chair | mega | pet ]";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                var shopId = -1;

                if (args[0] == "gear")
                {
                    shopId = 9999999;
                }
                else if (args[0] == "scrolls")
                {
                    shopId = 9999998;
                }
                else if (args[0] == "nx")
                {
                    shopId = 9999997;
                }
                else if (args[0] == "face")
                {
                    shopId = 9999996;
                }
                else if (args[0] == "ring")
                {
                    shopId = 9999995;
                }
                else if (args[0] == "chair")
                {
                    shopId = 9999994;
                }
                else if (args[0] == "mega")
                {
                    shopId = 9999993;
                }
                else if (args[0] == "pet")
                {
                    shopId = 9999992;
                }

                if (shopId == -1)
                {
                    ShowSyntax(caller);
                }

                // TODO: Shop the desired shop.
                // As we assign shops to NPCs, we need to modify the MCDB values
                // so each shop will be matched to a different exclusive NPC.
            }
        }
    }
}
