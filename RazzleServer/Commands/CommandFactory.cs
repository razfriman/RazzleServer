using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Util;

namespace RazzleServer.Commands
{
    public static class CommandFactory
    {
        public static Commands Commands { get; private set; }

        private static ILogger Log = LogManager.Log;

        public static void Initialize()
        {
            Commands = new Commands();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(Command)))
                {
                    Commands.Add((Command)Activator.CreateInstance(type));
                }
            }
        }

        public static void Execute(MapleCharacter caller, string text)
        {
            string[] splitted = text.Split(' ');

            splitted[0] = splitted[0].ToLower();

            string commandName = splitted[0].TrimStart(ServerConfig.Instance.CommandIndiciator);

            string[] args = new string[splitted.Length - 1];

            for (int i = 1; i < splitted.Length; i++)
            {
                args[i - 1] = splitted[i];
            }

            if (Commands.Contains(commandName))
            {
                Command command = Commands[commandName];

                if (!command.IsRestricted || caller.IsAdmin)
                {
                    try
                    {
                        command.Execute(caller, args);
                    }
                    catch (Exception e)
                    {
                        caller.SendMessage("[Command] Unknown error: " + e.Message);
                        Log.Error("{0} error by {1}: ", e, command.GetType().Name, caller.Name);
                    }
                }
                else
                {
                    caller.SendMessage("[Command] Restricted command.");
                }
            }
            else
            {
                caller.SendMessage("[Command] Invalid command.");
            }
        }
    }
}
