using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripts.Command{
    public class PacketCommand : ACommandScript
    {
        public override bool IsRestricted { get { return true; } }
        public override string Name { get { return "packet"; } }
        public override string Parameters { get { return "{ client | server } packet"; } }

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 3)
            {
                ShowSyntax(caller);
            }
            else
            {
                var packet = Functions.Fuse(args, 1);
                if (args[0].ToLower() == "server")
                {
                    caller.Client.Send(Functions.HexToBytes(packet));
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