using System;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public class SkillReference
    {
        public sbyte MobCount { get; set; }
        public sbyte HitCount { get; set; }
        public short Range { get; set; }
        public int BuffTime { get; set; }
        public short CostMP { get; set; }
        public short CostHP { get; set; }
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
        public short HP { get; set; }
        public short MP { get; set; }
        public short Probability { get; set; }
        public short Morph { get; set; }
        public Point LT { get; private set; }
        public Point RB { get; private set; }
        public int Cooldown { get; set; }

        public SkillReference()
        {
        }

        public SkillReference(WzImageProperty img)
        {
            //MapleId = (int)datum["skillid"];
            //CurrentLevel = (byte)(short)datum["skill_level"];
            //MobCount = (sbyte)datum["mob_count"];
            //HitCount = (sbyte)datum["hit_count"];
            //Range = (short)datum["range"];
            //BuffTime = (int)datum["buff_time"];
            //CostHP = (short)datum["hp_cost"];
            //CostMP = (short)datum["mp_cost"];
            //Damage = (short)datum["damage"];
            //FixedDamage = (int)datum["fixed_damage"];
            //CriticalDamage = (byte)datum["critical_damage"];
            //Mastery = (sbyte)datum["mastery"];
            //OptionalItemCost = (int)datum["optional_item_cost"];
            //CostItem = (int)datum["item_cost"];
            //ItemCount = (short)datum["item_count"];
            //CostBullet = (short)datum["bullet_cost"];
            //CostMeso = (short)datum["money_cost"];
            //ParameterA = (short)datum["x_property"];
            //ParameterB = (short)datum["y_property"];
            //Speed = (short)datum["speed"];
            //Jump = (short)datum["jump"];
            //Strength = (short)datum["str"];
            //WeaponAttack = (short)datum["weapon_atk"];
            //MagicAttack = (short)datum["magic_atk"];
            //WeaponDefense = (short)datum["weapon_def"];
            //MagicDefense = (short)datum["magic_def"];
            //Accuracy = (short)datum["accuracy"];
            //Avoidability = (short)datum["avoid"];
            //HP = (short)datum["hp"];
            //MP = (short)datum["mp"];
            //Probability = (short)datum["prop"];
            //Morph = (short)datum["morph"];
            //LT = new Point((short)datum["ltx"], (short)datum["lty"]);
            //RB = new Point((short)datum["rbx"], (short)datum["rby"]);
            //Cooldown = (int)datum["cooldown_time"];
        }
    }
}
