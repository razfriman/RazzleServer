using RazzleServer.Common.Server;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class ACommandScript
    {
        public abstract string Name { get; }
        public abstract string Parameters { get; }
        public abstract bool IsRestricted { get; }

        public abstract void Execute(Character caller, string[] args);

        public void ShowSyntax(Character caller) => caller.Notify($"[Syntax] {ServerConfig.Instance.CommandIndicator}{Name} {Parameters}");
    }
}
