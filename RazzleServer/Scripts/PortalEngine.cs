﻿using Microsoft.Extensions.Logging;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Player;
using RazzleServer.Util;
using System;

namespace RazzleServer.Scripts
{
    public static class PortalEngine
    {
        private static readonly ILogger Log = LogManager.Log;

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
                        Log.LogError($"Error loading [PortalScript] [{portal.Script}]");
                        return;
                    }
                    scriptInstance.Character = new ScriptCharacter(character, portal.Script);
                    try
                    {
                        scriptInstance.Execute();
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, $"Portal Script Error [{0}]");
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