using Microsoft.Extensions.Logging;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Map;
using RazzleServer.Map.Monster;
using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Drawing;

namespace RazzleServer.Scripts
{
    public class EventEngine
    {
        private EventScript EventInstance;
        private readonly bool RecreatedMap;
        private readonly Type EventType;
        private readonly MapleCharacter Starter;
        private MapleEvent EventMap;
        public int EventId { get; }
        public byte ChannelId { get; }

        private static ILogger Log = LogManager.Log;

        public EventEngine(MapleCharacter starter, string script, int recreateMap = -1, bool skipSpawn = false)
        {
            Starter = starter;
            ChannelId = Starter.Client.Channel;
            if (!(DataBuffer.EventScripts.TryGetValue(script, out EventType) && EventType != null))
                return;
            EventInstance = ScriptActivator.CreateScriptInstance(EventType, script, starter) as EventScript;
            if (EventInstance == null)
            {
                Log.LogError($"Error loading [EventScript] {script}");
                return;
            }
            RecreatedMap = recreateMap != -1;
            EventInstance.OnFinish += new Action(FinishEvent);
            EventInstance.OnSpawnMobs += new Action<int, int, int, int>(SpawnMobs);
            EventInstance.OnRandomSpawnMobs += new Action<int, int, Point, Point>(RandomSpawnMobs);
            if (RecreatedMap)
            {
                EventMap = new MapleEvent(recreateMap, DataBuffer.GetMapById(recreateMap), skipSpawn);
                EventId = ServerManager.RegisterEvent(EventMap);
                if (Starter != null)
                    AddCharacter(Starter);
            }
        }

        public void StartEvent()
        {
            EventInstance.Execute();
        }

        public void EndEvent()
        {
            EventInstance.Finish();
        }

        private void FinishEvent()
        {
            ServerManager.UnregisterEvent(EventId);
            EventInstance = null;
            EventMap = null;
        }

        private void RandomSpawnMobs(int mobId, int count, Point maxPos, Point minPos)
        {
            WzMob mobInfo = DataBuffer.GetMobById(mobId);
            for (int i = 0; i < count; i++)
            {
                MapleMap spawnMap = null;
                if (RecreatedMap)
                    spawnMap = EventMap;
                else if (Starter.Map != null)
                    spawnMap = Starter.Map;
                if (spawnMap == null) return;

                WzMap.FootHold randomFh = spawnMap.GetRandomFoothold(maxPos, minPos);

                spawnMap.SpawnMobOnGroundBelow(new MapleMonster(mobInfo, spawnMap), randomFh.Point1);
            }
        }

        private void SpawnMobs(int mobId, int count, int x, int y)
        {
            WzMob mobInfo = DataBuffer.GetMobById(mobId);
            for (int i = 0; i < count; i++)
            {
                MapleMap spawnMap = null;
                if (RecreatedMap)
                    spawnMap = EventMap;
                else if (Starter.Map != null)
                    spawnMap = Starter.Map;
                if (spawnMap == null) return;

                spawnMap.SpawnMobOnGroundBelow(new MapleMonster(mobInfo, spawnMap), new Point(x, y));
            }
        }

        /// <summary>
        /// Only needed when the event requires data for the players, such as pq
        /// </summary>
        public void AddCharacter(MapleCharacter Character)
        {
            if (RecreatedMap)
            {
                MapleCharacter.EnterMap(Character.Client, EventMap.MapID, EventMap.GetDefaultSpawnPortal().Id);
                EventMap.AddCharacter(Character);
            }
            EventInstance.AddCharacter(new ScriptCharacter(Character, EventType.Name));
        }
    }
}
