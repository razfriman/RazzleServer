using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Parameters { get; }
        public abstract bool IsRestricted { get; }

        public abstract void Execute(MapleCharacter caller, string[] args);

        public void ShowSyntax(MapleCharacter caller)
        {
            caller.SendMessage($"[Syntax] {ServerConfig.Instance.CommandIndiciator}{Name} {Parameters}");
        }
    }
}
