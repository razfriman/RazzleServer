﻿using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Scripts.Commands
{
    public class PacketCommand : ACommandScript
    {
        public override bool IsRestricted => true;
        public override string Name => "packet";
        public override string Parameters => "{ client | server } packet";

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 3)
            {
                ShowSyntax(caller);
            }
            else
            {
                var packet = args.Fuse(1);
                if (args[0].ToLower() == "server")
                {
                    caller.Send(new PacketWriter(Functions.HexToBytes(packet)));
                }
                else if (args[0].ToLower().Equals("client"))
                {
                    caller.Client.Receive(new PacketReader(Functions.HexToBytes(packet)));
                }
                else
                {
                    ShowSyntax(caller);
                }
            }
        }
    }
}
