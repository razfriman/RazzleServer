using NLog;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Player;
using System;

namespace RazzleServer.Scripts
{
    public static class PortalEngine
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public static void EnterScriptedPortal(WzMap.Portal portal, MapleCharacter character)
        {
            if (!string.IsNullOrEmpty(portal.Script))
            {
                Type portalScriptType;
                if (DataBuffer.PortalScripts.TryGetValue(portal.Script, out portalScriptType) && portalScriptType != null)
                {
                    PortalScript scriptInstance = Activator.CreateInstance(portalScriptType) as PortalScript;
                    if (scriptInstance == null)
                    {
                        Log.Error($"Error loading [PortalScript] [{portal.Script}]");
                        return;
                    }
                    scriptInstance.Character = new ScriptCharacter(character, portal.Script);
                    try
                    {
                        scriptInstance.Execute();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, $"Portal Script Error [{0}]");
                        character.EnableActions();
                    }
                }
                else
                {
                    character.SendBlueMessage($"This portal is not coded yet (mapId: {character.MapID}, portalId: {portal.Id}, script name: {portal.Script})");
                    character.EnableActions();
                }
            }
            else
            {
                character.EnableActions();
            }
        }
    }
}