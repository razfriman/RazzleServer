using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class OnlineCommand : Command
    {
        public override string Name => "online";

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
                caller.Notify("[Online]");

                foreach (var channel in caller.Client.Server.World.Values)
                {
                    foreach (var channelClient in channel.Clients.Values)
                    {
                        caller.Notify("   -" + channelClient.Character.Name);
                    }
                }
            }
        }
    }
}
