using NLog;
using RazzleServer.Player;
using System;

namespace RazzleServer.Scripts
{
    public class ScriptActivator
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        //private static ScriptInterface ScriptDataProvider = new ScriptInterface();

        public static AMapleScript CreateScriptInstance(Type scriptType, string scriptName, MapleCharacter chr)
        {
            var instance = Activator.CreateInstance(scriptType) as AMapleScript;

            if (instance == null)
            {
                Log.Error($"Type [{scriptType}] cannot be cast to 'Script'");
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
