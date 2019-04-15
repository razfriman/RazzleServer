using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Items;

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
            var owner = GiveExperience(item);
            item.Attackers.Clear();
            GiveDrops(item, owner);
            UpdateQuestKills(item, owner);

            item.Controller.ControlledMobs.Remove(item);
            Map.Send(item.GetDestroyPacket());
            base.Remove(item);

            ScheduleRespawn(item);
            ProcessDeathSummons(item);
        }

        private void ProcessDeathSummons(Mob item)
        {
            foreach (var summonId in item.DeathSummons)
            {
                Map.Mobs.Add(new Mob(summonId) {Position = item.Position});
            }
        }

        private static void UpdateQuestKills(Mob item, Character owner)
        {
            if (owner == null)
            {
                return;
            }

            foreach (var loopStarted in owner.Quests.Started)
            {
                if (!loopStarted.Value.ContainsKey(item.MapleId))
                {
                    continue;
                }

                if (loopStarted.Value[item.MapleId] >=
                    CachedData.Quests.Data[loopStarted.Key].PostRequiredKills[item.MapleId])
                {
                    continue;
                }

                loopStarted.Value[item.MapleId]++;

                var kills = string.Empty;

                foreach (int kill in loopStarted.Value.Values)
                {
                    kills += kill.ToString().PadLeft(3, '0');
                }

                owner.Send(GamePackets.ShowStatusInfo(MessageType.QuestRecord,
                    mapleId: loopStarted.Key, questStatus: QuestStatus.InProgress, questString: kills));

                if (owner.Quests.CanComplete(loopStarted.Key, true))
                {
                    owner.Quests.NotifyComplete(loopStarted.Key);
                }
            }
        }

        private static void ScheduleRespawn(Mob item)
        {
            if (item.SpawnPoint != null)
            {
                TaskRunner.Run(item.SpawnPoint.Spawn, TimeSpan.FromSeconds(15));
            }
        }

        private void GiveDrops(Mob item, Character owner)
        {
            if (!item.CanDrop)
            {
                return;
            }

            var drops = CalculateDrops(item, owner);
            Map.Drops.SpawnDrops(drops, item.Position);
        }

        private List<Drop> CalculateDrops(Mob item, Character owner)
        {
            var drops = new List<Drop>();

            foreach (var loopLoot in item.CachedReference.Loots)
            {
                if (Functions.Random(1000000) / Map.Server.World.DropRate > loopLoot.Chance)
                {
                    continue;
                }

                if (loopLoot.IsMeso)
                {
                    drops.Add(new Meso(
                        (short)(Functions.Random(loopLoot.MinimumQuantity, loopLoot.MaximumQuantity) *
                                Map.Server.World.MesoRate)) {Dropper = item, Owner = owner});
                }
                else
                {
                    drops.Add(new Item(loopLoot.ItemId,
                        (short)Functions.Random(loopLoot.MinimumQuantity, loopLoot.MaximumQuantity))
                    {
                        Dropper = item, Owner = owner
                    });
                }
            }

            return drops;
        }

        private Character GiveExperience(Mob item)
        {
            Character owner = null;
            var mostDamage = 0u;

            foreach (var attacker in item.Attackers)
            {
                if (attacker.Key.Map != Map)
                {
                    continue;
                }

                if (attacker.Value > mostDamage)
                {
                    owner = attacker.Key;
                    mostDamage = attacker.Value;
                }

                attacker.Key.PrimaryStats.Experience +=
                    (int)Math.Min(item.Experience, attacker.Value * item.Experience / item.MaxHealth) *
                    attacker.Key.BaseClient.GameServer.World.ExperienceRate;
            }

            return owner;
        }
    }
}
