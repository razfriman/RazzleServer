﻿using RazzleServer.Game.Maple.Characters;
using RazzleServer.Server;

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
            string result = string.Empty;

            for (int i = start; i < args.Length; i++)
            {
                result += args[i] + ' ';
            }

            return result.Trim();
        }

        public string CombineArgs(string[] args, int start, int length)
        {
            string result = string.Empty;

            for (int i = start; i < length; i++)
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