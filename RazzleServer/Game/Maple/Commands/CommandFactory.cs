using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Commands
{
    public static class CommandFactory
    {
        public static Commands Commands { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public static void Initialize()
        {
            Commands = new Commands();

            var commandTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(Command)))
                    .ToList();

            commandTypes.ForEach(x => Commands.Add((Command)Activator.CreateInstance(x)));

            Log.LogInformation($"Loaded {commandTypes.Count} commands");
        }

        public static void Execute(Character caller, string text)
        {
            var splitted = text.Split(' ');

            splitted[0] = splitted[0].ToLower();

            var commandName = splitted[0].TrimStart(ServerConfig.Instance.CommandIndicator[0]);

            var args = new string[splitted.Length - 1];

            for (var i = 1; i < splitted.Length; i++)
            {
                args[i - 1] = splitted[i];
            }

            if (Commands.Contains(commandName))
            {
                var command = Commands[commandName];

                if (!command.IsRestricted || caller.IsMaster)
                {
                    try
                    {
                        command.Execute(caller, args);
                    }
                    catch (Exception e)
                    {
                        caller.Notify("[Command] Unknown error: " + e.Message);
                        Log.LogError($"{command.GetType().Name} error by {caller.Name}", e);
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
