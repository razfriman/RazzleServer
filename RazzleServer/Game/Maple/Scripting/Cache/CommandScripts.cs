using System;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using Serilog;

namespace RazzleServer.Game.Maple.Scripting.Cache
{
    public sealed class CommandScripts : MapleKeyedCollection<string, ACommandScript>
    {
        private readonly ILogger _log = Log.ForContext<CommandScripts>();

        public override string GetKey(ACommandScript item) => item.Name;

        public void Execute(Character caller, string text)
        {
            var splitted = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = splitted[0].ToLower().TrimStart(ServerConfig.Instance.CommandIndicator[0]);
            var args = splitted.AsSpan().Slice(1).ToArray();

            if (Contains(commandName))
            {
                var command = this[commandName];

                if (!command.IsRestricted || caller.IsMaster)
                {
                    try
                    {
                        command.Execute(caller, args);
                    }
                    catch (Exception e)
                    {
                        caller.Notify("[Command] Unknown error: " + e.Message);
                        _log.Error(e, $"{command.GetType().Name} error by {caller.Name}");
                    }
                }
                else
                {
                    caller.Notify("[Command] Restricted command.");
                }
            }
            else
            {
                caller.Notify("[Command] Invalid command.");
            }
        }
    }
}
