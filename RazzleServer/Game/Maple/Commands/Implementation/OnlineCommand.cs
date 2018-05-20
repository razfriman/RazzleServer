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

                //foreach (WorldServer world in MasterServer.Worlds)
                //{
                //    foreach (ChannelServer channel in world)
                //    {
                //        foreach (Character loopCharacter in channel.Characters)
                //        {
                //            caller.Notify("   -" + loopCharacter.Name);
                //        }
                //    }
                //}
            }
        }
    }
}
