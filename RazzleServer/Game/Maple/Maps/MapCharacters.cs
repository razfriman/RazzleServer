using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapCharacters : MapObjects<Character>
    {
        public MapCharacters(Map map) : base(map) { }

        public Character this[string name] => Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public override void Add(Character item)
        {
            lock (this)
            {
                foreach (var character in Values)
                {
                    item.Client.Send(character.GetSpawnPacket());
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
                Log.Error(e, "Error adding item to MapCharacters");
            }

            lock (Map.Drops)
            {
                foreach (var drop in Map.Drops.Values)
                {
                    item.Client.Send(drop.GetSpawnPacket(drop.Owner == null ? item : null));
                }
            }

            lock (Map.Mobs)
            {
                foreach (var mob in Map.Mobs.Values)
                {
                    item.Client.Send(mob.GetSpawnPacket());
                }
            }


            lock (Map.Npcs)
            {
                foreach (var npc in Map.Npcs.Values)
                {
                    item.Client.Send(npc.GetSpawnPacket());
                }
            }

            lock (Map.Reactors)
            {
                foreach (var reactor in Map.Reactors.Values)
                {
                    item.Client.Send(reactor.GetSpawnPacket());
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

        public override int GetKey(Character item) => item.Id;
    }
}
