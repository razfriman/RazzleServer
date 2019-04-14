using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class JobCommand : ACommandScript
    {
        public override string Name => "job";

        public override string Parameters => "{ id | name}";

        public override bool IsRestricted => true;

        public override void Execute(GameCharacter caller, string[] args)
        {
            if (args.Length != 1)
            {
                ShowSyntax(caller);
            }
            else
            {
                try
                {
                    var jobId = short.Parse(args[0]);

                    if (Enum.IsDefined(typeof(Job), jobId))
                    {
                        caller.PrimaryStats.Job = (Job)jobId;
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
                        caller.PrimaryStats.Job = (Job)Enum.Parse(typeof(Job), args[0], true);
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
