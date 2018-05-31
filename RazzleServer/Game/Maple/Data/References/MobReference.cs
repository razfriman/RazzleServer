using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MobReference
    {
        public int MapleId { get; set; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public List<Loot> Loots { get; set; }
        public short Foothold { get; set; }
        public List<MobSkillReference> Skills { get; set; } = new List<MobSkillReference>();
        public List<MobStatus> Buffs { get; set; }
        public List<int> DeathSummons { get; set; }
        public short Level { get; set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; set; }
        public uint MaxMana { get; set; }
        public uint HealthRecovery { get; set; }
        public uint ManaRecovery { get; set; }
        public int ExplodeHealth { get; set; }
        public uint Experience { get; set; }
        public int Link { get; set; }
        public short SummonType { get; set; }
        public int FixedDamage { get; set; }
        public int DeathBuff { get; set; }
        public int DeathAfter { get; set; }
        public double Traction { get; set; }
        public bool DamagedByMobOnly { get; set; }
        public int DropItemPeriod { get; set; }
        public byte HpBarForeColor { get; set; }
        public byte HpBarBackColor { get; set; }
        public byte CarnivalPoints { get; set; }
        public int WeaponAttack { get; set; }
        public int WeaponDefense { get; set; }
        public int MagicAttack { get; set; }
        public int MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Speed { get; set; }
        public short ChaseSpeed { get; set; }
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

            var info = img["info"];
            Level = info["level"]?.GetShort() ?? 1;
            MaxHealth = (uint)(info["maxHP"]?.GetInt() ?? 0);
            Health = MaxHealth;
            MaxMana = (uint)(info["maxMP"]?.GetInt() ?? 0);
            Mana = MaxHealth;
            Speed = info["speed"]?.GetShort() ?? 0;
            HpBarForeColor = (byte)(info["hpTagColor"]?.GetInt() ?? 0);
            HpBarBackColor = (byte)(info["hpTagBgcolor"]?.GetInt() ?? 0);
            SummonType = info["summonType"]?.GetShort() ?? 0;
            Link = info["link"]?.GetInt() ?? 0;
            WeaponAttack = info["PADamage"]?.GetInt() ?? 0;
            WeaponDefense = info["PDDamage"]?.GetInt() ?? 0;
            MagicAttack = info["MADamage"]?.GetInt() ?? 0;
            MagicDefense = info["MDDamage"]?.GetInt() ?? 0;
            Accuracy = info["acc"]?.GetShort() ?? 0;
            Avoidability = info["eva"]?.GetShort() ?? 0;
            Experience = (uint)(info["exp"]?.GetInt() ?? 0);
            HealthRecovery = (uint)(info["hpRecovery"]?.GetInt() ?? 0);
            ManaRecovery = (uint)(info["mpRecovery"]?.GetInt() ?? 0);
            ChaseSpeed = info["chaseSpeed"]?.GetShort() ?? 0;
            FixedDamage = info["fixedDamage"]?.GetInt() ?? 0;
            DropItemPeriod = info["dropItemPeriod"]?.GetInt() ?? 0;
            DamagedByMobOnly = (info["damagedByMob"]?.GetInt() ?? 0) > 0;
            Traction = info["fs"]?.GetDouble() ?? 0d;
            DeathAfter = info["removeAfter"]?.GetInt() ?? 0;

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
            DeathSummons = new List<int>();
            info["skill"]?.WzProperties.ForEach(x => Skills.Add(new MobSkillReference(x)));
            info["revive"]?.WzProperties?.ForEach(x => DeathSummons.Add(x.GetInt()));
        }
    }
}
