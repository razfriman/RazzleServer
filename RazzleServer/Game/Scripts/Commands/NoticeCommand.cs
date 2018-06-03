using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class NoticeCommand : ACommandScript
    {
        public override string Name => "notice";

        public override string Parameters => "{ -map | -channel | -world } message";

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length < 2)
            {
                ShowSyntax(caller);
            }
            else
            {
                var message = args.Fuse(1);

                switch (args[0].ToLower())
                {
                    case "-map":
                        caller.Map.Send(GamePackets.Notify(message));
                        break;

                    case "-channel":
                        caller.Client.Server.Send(GamePackets.Notify(message));
                        break;

                    case "-world":
                        caller.Client.Server.World.Send(GamePackets.Notify(message));
                        break;

                    default:
                        ShowSyntax(caller);
                        break;
                }
            }
        }
    }
}
