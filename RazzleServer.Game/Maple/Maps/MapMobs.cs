using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var (questId, questData) in owner.Quests.Started)
            {
                if (!questData.ContainsKey(item.MapleId))
                {
                    continue;
                }

                if (questData[item.MapleId] >=
                    CachedData.Quests.Data[questId].PostRequiredKills[item.MapleId])
                {
                    continue;
                }

                questData[item.MapleId]++;

                var kills = questData.Values.Cast<int>().Aggregate(string.Empty,
                    (current, kill) => current + kill.ToString().PadLeft(3, '0'));

                owner.Send(GamePackets.ShowStatusInfo(MessageType.QuestRecord,
                    mapleId: questId, questStatus: QuestStatus.InProgress, questString: kills));

                if (owner.Quests.CanComplete(questId, true))
                {
                    owner.Quests.NotifyComplete(questId);
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

            foreach (var (character, damage) in item.Attackers.Where(attacker => attacker.Key.Map == Map))
            {
                if (damage > mostDamage)
                {
                    owner = character;
                    mostDamage = damage;
                }

                character.PrimaryStats.Experience +=
                    (int)Math.Min(item.Experience, damage * item.Experience / item.MaxHealth) *
                    character.BaseClient.GameServer.World.ExperienceRate;
            }

            return owner;
        }
    }
}
