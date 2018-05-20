using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands.Implementation
{
    public sealed class JobCommand : Command
    {
        public override string Name
        {
            get
            {
                return "job";
            }
        }

        public override string Parameters
        {
            get
            {
                return "{ id | name}";
            }
        }

        public override bool IsRestricted
        {
            get
            {
                return true;
            }
        }

        public override void Execute(Character caller, string[] args)
        {
            if (args.Length != 1)
            {
                this.ShowSyntax(caller);
            }
            else
            {
                try
                {
                    short jobId = short.Parse(args[0]);

                    if (Enum.IsDefined(typeof(Job), jobId))
                    {
                        caller.Job = (Job)jobId;
                    }
                    else
                    {
                        caller.Notify("[Command] Invalid job Id.");
                    }
                }
                catch (FormatException)
                {
                    try
                    {
                        caller.Job = (Job)Enum.Parse(typeof(Job), args[0], true);
                    }
                    catch (ArgumentException)
                    {
                        caller.Notify("[Command] Invalid job name.");
                    }
                }
            }
        }
    }
}
