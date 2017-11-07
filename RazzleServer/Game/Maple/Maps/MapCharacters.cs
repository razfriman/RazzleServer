using System.Collections.Generic;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapCharacters : MapObjects<Character>
    {
        public MapCharacters(Map map) : base(map) { }

        public Character this[string name]
        {
            get
            {
                foreach (Character character in this)
                {
                    if (character.Name.ToLower() == name.ToLower())
                    {
                        return character;
                    }
                }

                return null;
            }
        }

        protected override void InsertItem(int index, Character item)
        {
            lock (this)
            {
                foreach (Character character in this)
                {
                    item.Client.Send(character.GetSpawnPacket());
                }
            }

            item.Position = Map.Portals.Count > 0 ? Map.Portals[item.SpawnPoint].Position : new Point(0, 0);

            base.InsertItem(index, item);

            lock (Map.Drops)
            {
                foreach (Drop drop in Map.Drops)
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
                foreach (Mob mob in Map.Mobs)
                {
                    item.Client.Send(mob.GetSpawnPacket());
                }
            }

            lock (Map.Npcs)
            {
                foreach (Npc npc in Map.Npcs)
                {
                    item.Client.Send(npc.GetSpawnPacket());
                }
            }

            lock (Map.Reactors)
            {
                foreach (Reactor reactor in Map.Reactors)
                {
                    item.Client.Send(reactor.GetSpawnPacket());
                }
            }

            lock (Map.Mobs)
            {
                foreach (Mob mob in Map.Mobs)
                {
                    mob.AssignController();
                }
            }

            lock (Map.Npcs)
            {
                foreach (Npc npc in Map.Npcs)
                {
                    npc.AssignController();
                }
            }

            Map.Broadcast(item.GetCreatePacket(), item);
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                Character item = base.Items[index];

                item.ControlledMobs.Clear();
                item.ControlledNpcs.Clear();

                Map.Broadcast(item.GetDestroyPacket(), item);
            }

            base.RemoveItem(index);

            lock (Map.Mobs)
            {
                foreach (Mob mob in Map.Mobs)
                {
                    mob.AssignController();
                }
            }

            lock (Map.Npcs)
            {
                foreach (Npc npc in Map.Npcs)
                {
                    npc.AssignController();
                }
            }
        }

        protected override int GetKeyForItem(Character item)
        {
            return item.ID;
        }
    }
}
