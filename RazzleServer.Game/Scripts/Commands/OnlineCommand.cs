using System.Linq;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class OnlineCommand : ACommandScript
    {
        public override string Name => "online";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
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
                    foreach (var channelClient in channel.Clients.Values.Cast<GameClient>())
                    {
                        caller.Notify("   -" + channelClient.GameCharacter.Name);
                    }
                }
            }
        }
    }
}
