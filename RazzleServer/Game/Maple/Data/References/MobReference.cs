using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data
{
    public class MobReference
    {
        public int MapleId { get; private set; }
        public SpawnPoint SpawnPoint { get; private set; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public List<Loot> Loots { get; private set; }
        public short Foothold { get; set; }
        public MobSkills Skills { get; private set; }
        public Dictionary<MobSkill, DateTime> Cooldowns { get; private set; }
        public List<MobStatus> Buffs { get; private set; }
        public List<int> DeathSummons { get; private set; }

        public short Level { get; private set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; private set; }
        public uint MaxMana { get; private set; }
        public uint HealthRecovery { get; private set; }
        public uint ManaRecovery { get; private set; }
        public int ExplodeHealth { get; private set; }
        public uint Experience { get; private set; }
        public int Link { get; private set; }
        public short SummonType { get; private set; }
        public int FixedDamage { get; private set; }
        public int DeathBuff { get; private set; }
        public int DeathAfter { get; private set; }
        public double Traction { get; private set; }
        public bool DamagedByMobOnly { get; private set; }
        public int DropItemPeriod { get; private set; }
        public byte HpBarForeColor { get; private set; }
        public byte HpBarBackColor { get; private set; }
        public byte CarnivalPoints { get; private set; }
        public int WeaponAttack { get; private set; }
        public int WeaponDefense { get; private set; }
        public int MagicAttack { get; private set; }
        public int MagicDefense { get; private set; }
        public short Accuracy { get; private set; }
        public short Avoidability { get; private set; }
        public short Speed { get; private set; }
        public short ChaseSpeed { get; private set; }

        public bool IsFacingLeft => Stance % 2 == 0;


        public MobReference()
        {
        }

        public MobReference(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;

            Level = img["level"]?.GetShort() ?? 1;
            MaxHealth = (uint)(img["maxHP"]?.GetInt() ?? 0);
            Health = MaxHealth;
            MaxMana = (uint)(img["maxMP"]?.GetInt() ?? 0);
            Mana = MaxHealth;
            Speed = img["speed"]?.GetShort() ?? 0;
            HpBarForeColor = (byte)(img["hpTagColor"]?.GetInt() ?? 0);
            HpBarBackColor = (byte)(img["hpTagBgcolor"]?.GetInt() ?? 0);
            SummonType = img["summonType"]?.GetShort() ?? 0;
            Link = img["link"]?.GetInt() ?? 0;
            WeaponAttack = img["PADamage"]?.GetInt() ?? 0;
            WeaponDefense = img["PDDamage"]?.GetInt() ?? 0;
            MagicAttack = img["MADamage"]?.GetInt() ?? 0;
            MagicDefense = img["MDDamage"]?.GetInt() ?? 0;
            Accuracy = img["acc"]?.GetShort() ?? 0;
            Avoidability = img["eva"]?.GetShort() ?? 0;
            Experience = (uint)(img["exp"]?.GetInt() ?? 0);
            HealthRecovery = (uint)(img["hpRecovery"]?.GetInt() ?? 0);
            ManaRecovery = (uint)(img["mpRecovery"]?.GetInt() ?? 0);
            ChaseSpeed = img["chaseSpeed"]?.GetShort() ?? 0;
            FixedDamage = img["fixedDamage"]?.GetInt() ?? 0;
            DropItemPeriod = img["dropItemPeriod"]?.GetInt() ?? 0;
            DamagedByMobOnly = (img["damagedByMob"]?.GetInt() ?? 0) > 0;
            Traction = img["fs"]?.GetDouble() ?? 0d;
            DeathAfter = img["removeAfter"]?.GetInt() ?? 0;

            //publicReward
            //explosiveReward
            //HPgaugeHide
            //firstAttack
            //boss
            //undead
            //pushed
            //bodyAttack
            //elemAttr
            //noFlip
            //damagedBySelectedMob/0 = 9300150
            //doNotRemove
            //onlyNormalAttack
            //buff

            Loots = new List<Loot>();
            Skills = new MobSkills(null);
            DeathSummons = new List<int>();

            img["skill"]?.WzProperties.ForEach(x => Skills.Add(new MobSkill(x)));
            img["revive"]?.WzProperties?.ForEach(x => DeathSummons.Add(x.GetInt()));
        }
    }
}
