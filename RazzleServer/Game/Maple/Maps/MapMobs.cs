using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Server;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Common;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapMobs : MapObjects<Mob>
    {
        public MapMobs(Map map) : base(map) { }

        public override void Add(Mob item)
        {
            base.Add(item);
            Map.Send(item.GetCreatePacket());
            item.AssignController();
        }

        public override void Remove(Mob item)
        {
            var mostDamage = 0;
            Character owner = null;

            foreach (var attacker in item.Attackers)
            {
                if (attacker.Key.Map == Map)
                {
                    if (attacker.Value > mostDamage)
                    {
                        owner = attacker.Key;
                    }

                    attacker.Key.Experience += (int)Math.Min(item.Experience, attacker.Value * item.Experience / item.MaxHealth) * attacker.Key.Client.Server.World.ExperienceRate;
                }
            }

            item.Attackers.Clear();

            if (item.CanDrop)
            {
                var drops = new List<Drop>();

                foreach (var loopLoot in item.Loots)
                {
                    if (Functions.Random(1000000) / Map.Server.World.DropRate <= loopLoot.Chance)
                    {
                        if (loopLoot.IsMeso)
                        {
                            drops.Add(new Meso((short)(Functions.Random(loopLoot.MinimumQuantity, loopLoot.MaximumQuantity) * Map.Server.World.MesoRate))
                            {
                                Dropper = item,
                                Owner = owner
                            });
                        }
                        else
                        {
                            drops.Add(new Item(loopLoot.ItemId, (short)Functions.Random(loopLoot.MinimumQuantity, loopLoot.MaximumQuantity))
                            {
                                Dropper = item,
                                Owner = owner
                            });
                        }
                    }
                }

                foreach (var loopDrop in drops)
                {
                    // TODO: Space out drops.

                    Map.Drops.Add(loopDrop);
                }
            }

            if (owner != null)
            {
                foreach (var loopStarted in owner.Quests.Started)
                {
                    if (loopStarted.Value.ContainsKey(item.MapleId))
                    {
                        if (loopStarted.Value[item.MapleId] < DataProvider.Quests.Data[loopStarted.Key].PostRequiredKills[item.MapleId])
                        {
                            loopStarted.Value[item.MapleId]++;

                            var kills = string.Empty;

                            foreach (int kill in loopStarted.Value.Values)
                            {
                                kills += kill.ToString().PadLeft(3, '0');
                            }

                            owner.Client.Send(GamePackets.ShowStatusInfo(MessageType.QuestRecord, mapleId: loopStarted.Key, questStatus: QuestStatus.InProgress, questString: kills));

                            if (owner.Quests.CanComplete(loopStarted.Key, true))
                            {
                                owner.Quests.NotifyComplete(loopStarted.Key);
                            }
                        }
                    }
                }
            }

            item.Controller.ControlledMobs.Remove(item);
            Map.Send(item.GetDestroyPacket());

            base.Remove(item);

            if (item.SpawnPoint != null)
            {
                Delay.Execute(item.SpawnPoint.Spawn, 3 * 1000); // TODO: Actual respawn time.
            }

            foreach (var summonId in item.DeathSummons)
            {
                Map.Mobs.Add(new Mob(summonId)
                {
                    Position = item.Position // TODO: Set owner as well.
                });
            }
        }
    }
}
