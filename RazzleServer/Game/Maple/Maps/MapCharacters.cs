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

                throw new KeyNotFoundException();
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
                        using (PacketReader oPacket = drop.GetSpawnPacket(item))
                        {
                            item.Client.Send(oPacket);
                        }
                    }
                    else
                    {
                        using (PacketReader oPacket = drop.GetSpawnPacket())
                        {
                            item.Client.Send(oPacket);
                        }
                    }
                }
            }

            lock (Map.Mobs)
            {
                foreach (Mob mob in Map.Mobs)
                {
                    using (PacketReader oPacket = mob.GetSpawnPacket())
                    {
                        item.Client.Send(oPacket);
                    }
                }
            }

            lock (Map.Npcs)
            {
                foreach (Npc npc in Map.Npcs)
                {
                    using (PacketReader oPacket = npc.GetSpawnPacket())
                    {
                        item.Client.Send(oPacket);
                    }
                }
            }

            lock (Map.Reactors)
            {
                foreach (Reactor reactor in Map.Reactors)
                {
                    using (PacketReader oPacket = reactor.GetSpawnPacket())
                    {
                        item.Client.Send(oPacket);
                    }
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

            using (PacketReader oPacket = item.GetCreatePacket())
            {
                Map.Broadcast(oPacket, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                Character item = base.Items[index];

                item.ControlledMobs.Clear();
                item.ControlledNpcs.Clear();

                using (PacketReader oPacket = item.GetDestroyPacket())
                {
                    Map.Broadcast(oPacket, item);
                }
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
