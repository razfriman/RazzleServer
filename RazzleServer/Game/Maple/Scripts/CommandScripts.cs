using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripts
{
    public sealed class CommandScripts : MapleKeyedCollection<string, ACommandScript>
    {
        private readonly ILogger _log = LogManager.Log;

        public override string GetKey(ACommandScript item) => item.Name;

        public void Execute(Character caller, string text)
        {
            var splitted = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            splitted[0] = splitted[0].ToLower();

            var commandName = splitted[0].TrimStart(ServerConfig.Instance.CommandIndicator[0]);

            var args = new string[splitted.Length - 1];

            for (var i = 1; i < splitted.Length; i++)
            {
                args[i - 1] = splitted[i];
            }

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
                        _log.LogError($"{command.GetType().Name} error by {caller.Name}", e);
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
