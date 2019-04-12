﻿using System;
using System.Linq;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Common.Util;
using Serilog;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapCharacters : MapObjects<Character>
    {
        private readonly ILogger _log = Log.ForContext<MapCharacters>();

        public MapCharacters(Map map) : base(map) { }

        public Character this[string name] =>
            Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public override void Add(Character item)
        {
            lock (this)
            {
                foreach (var character in Values)
                {
                    item.Send(character.GetSpawnPacket());
                }
            }

            item.Position = Map.Portals.Count > item.SpawnPoint
                ? Map.Portals[item.SpawnPoint].Position
                : new Point(0, 0);

            try
            {
                base.Add(item);
            }
            catch (Exception e)
            {
                _log.Error(e, "Error adding item to MapCharacters");
            }

            lock (Map.Drops)
            {
                foreach (var drop in Map.Drops.Values)
                {
                    item.Send(drop.GetSpawnPacket(drop.Owner == null ? item : null));
                }
            }

            lock (Map.Mobs)
            {
                foreach (var mob in Map.Mobs.Values)
                {
                    item.Send(mob.GetSpawnPacket());
                }
            }


            lock (Map.Npcs)
            {
                foreach (var npc in Map.Npcs.Values)
                {
                    item.Send(npc.GetSpawnPacket());
                }
            }

            lock (Map.Mobs)
            {
                foreach (var mob in Map.Mobs.Values)
                {
                    mob.AssignController();
                }
            }

            lock (Map.Npcs)
            {
                foreach (var npc in Map.Npcs.Values)
                {
                    npc.AssignController();
                }
            }
            
            if (Map.CachedReference.TimeLimit > 0)
            {
                //TODO(raz): Show the timer
            }

            Map.Send(item.GetCreatePacket(), item);
        }

        public override void Remove(Character item)
        {
            lock (this)
            {
                item.ControlledMobs.Clear();
                item.ControlledNpcs.Clear();
                Map.Send(item.GetDestroyPacket(), item);
            }

            base.Remove(item);

            lock (Map.Mobs)
            {
                foreach (var mob in Map.Mobs.Values)
                {
                    mob.AssignController();
                }
            }

            lock (Map.Npcs)
            {
                foreach (var npc in Map.Npcs.Values)
                {
                    npc.AssignController();
                }
            }
        }

        public void Hide(Character item) => Map.Send(item.GetDestroyPacket(), item);

        public void Show(Character item) => Map.Send(item.GetCreatePacket(), item);

        public override int GetKey(Character item) => item.Id;
    }
}
