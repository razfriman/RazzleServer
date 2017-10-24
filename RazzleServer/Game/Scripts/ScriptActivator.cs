using Microsoft.Extensions.Logging;
using RazzleServer.Player;
using System;
using RazzleServer.Util;

namespace RazzleServer.Scripts
{
    public static class ScriptActivator
    {
        private static readonly ILogger Log = LogManager.Log;

        public static AMapleScript CreateScriptInstance(Type scriptType, string scriptName, MapleCharacter chr)
        {
            var instance = Activator.CreateInstance(scriptType) as AMapleScript;

            if (instance == null)
            {
                Log.LogError($"Type [{scriptType}] cannot be cast to 'Script'");
                return null;
            }
            //if (instance is CharacterScript)
            //{
            //    CharacterScript cInstance = (CharacterScript)instance;
            //    cInstance.Character = new ScriptCharacter(chr, scriptName);
            //}
            //instance.DataProvider = ScriptDataProvider;
            return instance;
        }
    }
}
