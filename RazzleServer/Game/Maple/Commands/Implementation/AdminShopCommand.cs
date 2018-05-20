using System;
using System.Collections.Generic;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class AdminShopCommand : Command
    {
        // NOTE: The Npc that is the shop owner.
        public const int Npc = 2084001;

        // TODO: Make a separate class called AdminShopItem to hold these values.
        // We can either make the items constant or load them from SQL.
        // As you can edit them in-game, I think SQL would be better.
        // In order: Id, MapleId, Price, Stock.
        public static List<Tuple<int, int, int, short>> Items = new List<Tuple<int, int, int, short>>()
        {
            new Tuple<int, int, int, short>(0, 2000000, 1000, 200),
            new Tuple<int, int, int, short>(1, 2000001, 1000, 200),
            new Tuple<int, int, int, short>(2, 2000002, 1000, 200)
        };

        public override string Name => "adminshop";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 0)
            {
                ShowSyntax(caller);
            }
            else
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.AdminShop))
                {

                    oPacket.WriteInt(Npc);
                    oPacket.WriteShort((short)Items.Count);

                    foreach (var item in Items)
                    {

                        oPacket.WriteInt(item.Item1);
                        oPacket.WriteInt(item.Item2);
                        oPacket.WriteInt(item.Item3);
                        oPacket.WriteByte(0); // NOTE: Unknown.
                        oPacket.WriteShort(item.Item4);
                    }

                    // NOTE: If enabled, when you exit the shop the NPC will ask you if you were looking for something that was missing.
                    // If you press yes, a search box with all the items in game will pop up and you can select an item to "register".
                    // Once you register an item, a packet will be sent to the server with it's Id so it can be added to the shop.
                    oPacket.WriteBool(true);

                    caller.Client.Send(oPacket);
                }
            }
        }
    }
}
