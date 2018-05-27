using System.Linq;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapCharacters : MapObjects<Character>
    {
        public MapCharacters(Map map) : base(map) { }

        public Character this[string name]
        {
            get => Values.FirstOrDefault(x => x.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        }

        public override void OnItemAdded(Character item)
        {
            lock (this)
            {
                foreach (var character in Values)
                {
                    item.Client.Send(character.GetSpawnPacket());
                }
            }

            item.Position = Map.Portals.Count > 0 ? Map.Portals[item.SpawnPoint].Position : new Point(0, 0);

            lock (Map.Drops)
            {
                foreach (var drop in Map.Drops.Values)
                {
                    if (drop.Owner == null)
                    {
                        item.Client.Send(drop.GetSpawnPacket(item));
                    }
                    else
                    {
                        item.Client.Send(drop.GetSpawnPacket());
                    }
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

            Map.Send(item.GetCreatePacket());
        }

        public override void OnItemRemoved(Character item)
        {
            lock (this)
            {
                item.ControlledMobs.Clear();
                item.ControlledNpcs.Clear();

                Map.Send(item.GetDestroyPacket(), item);
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
        }

        public override int GetId(Character item) => item.Id;
    }
}
