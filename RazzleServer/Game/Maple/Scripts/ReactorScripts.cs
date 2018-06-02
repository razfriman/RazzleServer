using System;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Scripts
{
    public sealed class ReactorScripts : MapleKeyedCollection<string, AReactorScript>
    {
        private readonly ILogger _log = LogManager.Log;

        public override string GetKey(AReactorScript item) => item.Name;

        public void Execute(Character caller, string text)
        {
            if (Contains(commandName))
            {
                var script = this[commandName];

                if (!script.IsRestricted || caller.IsMaster)
                {
                    try
                    {
                        script.Execute(caller, args);
                    }
                    catch (Exception e)
                    {
                        caller.Notify("[Reactor] Unknown error: " + e.Message);
                        _log.LogError($"{script.GetType().Name} error by {caller.Name}", e);
                    }
                }
                else
                {
                    caller.Notify("[Reactor] Restricted command.");
                }
            }
            else
            {
                caller.Notify("[Reactor] Invalid command.");
            }
        }
    }
}
