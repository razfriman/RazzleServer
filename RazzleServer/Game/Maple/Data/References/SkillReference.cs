using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class SkillReference
    {
        public sbyte MobCount { get; set; }
        public sbyte HitCount { get; set; }
        public short Range { get; set; }
        public int BuffTime { get; set; }
        public short CostMp { get; set; }
        public short CostHp { get; set; }
        public short Damage { get; set; }
        public int FixedDamage { get; set; }
        public byte CriticalDamage { get; set; }
        public sbyte Mastery { get; set; }
        public int OptionalItemCost { get; set; }
        public int CostItem { get; set; }
        public short ItemCount { get; set; }
        public short CostBullet { get; set; }
        public short CostMeso { get; set; }
        public short ParameterA { get; set; }
        public short ParameterB { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Strength { get; set; }
        public short WeaponAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicAttack { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Hp { get; set; }
        public short Mp { get; set; }
        public short Probability { get; set; }
        public short Morph { get; set; }
        public Point? Lt { get; private set; }
        public Point? Rb { get; private set; }
        public SkillReference()
        {
        }

        public SkillReference(WzImageProperty img)
        {
            ParameterA = img["x"]?.GetShort() ?? 0;
            ParameterB = img["y"]?.GetShort() ?? 0;
            CostMp = img["mpCon"]?.GetShort() ?? 0;
            CostHp = img["hpCon"]?.GetShort() ?? 0;
            WeaponDefense = img["pdd"]?.GetShort() ?? 0;
            MagicDefense = img["mdd"]?.GetShort() ?? 0;
            WeaponAttack = img["pad"]?.GetShort() ?? 0;
            MagicAttack = img["mad"]?.GetShort() ?? 0;
            BuffTime = img["time"]?.GetInt() ?? 0;
            Damage = img["damage"]?.GetShort() ?? 0;
            Range = img["range"]?.GetShort() ?? 0;
            MobCount = (sbyte)(img["mobCount"]?.GetShort() ?? 0);
            HitCount = (sbyte)(img["attackCount"]?.GetShort() ?? 0);
            Lt = img["lt"]?.GetPoint();
            Rb = img["rb"]?.GetPoint();
            Mastery = (sbyte)(img["mastery"]?.GetShort() ?? 0);
            Speed = img["speed"]?.GetShort() ?? 0;
            CostItem = img["itemCon"]?.GetInt() ?? 0;
            ItemCount = img["itemConNo"]?.GetShort() ?? 0;
            CostBullet = img["bulletCount"]?.GetShort() ?? 0;
            Jump = img["jump"]?.GetShort() ?? 0;
            Accuracy = img["acc"]?.GetShort() ?? 0;
            Avoidability = img["eva"]?.GetShort() ?? 0;
            Probability = img["prop"]?.GetShort() ?? 0;
            FixedDamage = img["fixDamage"]?.GetInt() ?? 0;
            CriticalDamage = (byte)(img["criticalDamage"]?.GetInt() ?? 0);
            CostMeso = img["moneyCon"]?.GetShort() ?? 0;
            Hp = img["hp"]?.GetShort() ?? 0;
            Mp = img["mp"]?.GetShort() ?? 0;
            //Strength = "str";
            //Morph = "morph";
        }
    }
}
