﻿using RazzleServer.Game.Maple.Characters;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class HelpCommand : Command
    {
        public override string Name
        {
            get
            {
                return "help";
            }
        }

        public override string Parameters
        {
            get
            {
                return string.Empty;
            }
        }

        public override bool IsRestricted
        {
            get
            {
                return false;
            }
        }

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 0)
            {
                this.ShowSyntax(caller);
            }
            else
            {
                caller.Notify("[Help]");

                foreach (Command command in CommandFactory.Commands)
                {
                    if ((command.IsRestricted && caller.IsMaster) || !command.IsRestricted && !(command is HelpCommand))
                    {
                        caller.Notify($"{ServerConfig.Instance.CommandIndicator} {command.Name} {command.Parameters}");
                    }
                }
            }
        }
    }
}
