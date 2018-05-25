using RazzleServer.Center;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Parameters { get; }
        public abstract bool IsRestricted { get; }

        public abstract void Execute(Character caller, string[] args);

        public string CombineArgs(string[] args, int start = 0)
        {
            var result = string.Empty;

            for (var i = start; i < args.Length; i++)
            {
                result += args[i] + ' ';
            }

            return result.Trim();
        }

        public string CombineArgs(string[] args, int start, int length)
        {
            var result = string.Empty;

            for (var i = start; i < length; i++)
            {
                result += args[i] + ' ';
            }

            return result.Trim();
        }

        public void ShowSyntax(Character caller)
        {
            caller.Notify($"[Syntax] {ServerConfig.Instance.CommandIndicator}{Name}");
        }
    }
}
