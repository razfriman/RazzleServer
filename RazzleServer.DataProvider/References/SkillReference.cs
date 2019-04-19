using RazzleServer.Common.Util;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    public class SkillReference
    {
        private readonly ILogger _log = Log.ForContext<SkillReference>();

        public int MapleId { get; set; }
        public byte Level { get; set; }
        public sbyte MobCount { get; set; }
        public sbyte HitCount { get; set; }
        public short Range { get; set; }
        public int BuffTime { get; set; }
        public short CostMp { get; set; }
        public short CostHp { get; set; }
        public short Damage { get; set; }
        public byte CriticalDamage { get; set; }
        public sbyte Mastery { get; set; }
        public int OptionalItemCost { get; set; }
        public int CostItem { get; set; }
        public short ItemCount { get; set; }
        public short CostBullet { get; set; }
        public short CostMeso { get; set; }
        public short ParameterA { get; set; }
        public short ParameterB { get; set; }
        public short ParameterC { get; set; }
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
        public Point? Lt { get; private set; }
        public Point? Rb { get; private set; }

        public SkillReference()
        {
        }

        public SkillReference(int mapleId, byte level, WzImageProperty img)
        {
            MapleId = mapleId;
            Level = level;

            foreach (var node in img.WzPropertiesList)
            {
                switch (node.Name)
                {
                    case "hs":
                    case "hit":
                    case "ball":
                    case "action":
                    case "58":
                        break;
                    case "x":
                        ParameterA = node.GetShort();
                        break;
                    case "y":
                        ParameterB = node.GetShort();
                        break;
                    case "z":
                        ParameterC = node.GetShort();
                        break;
                    case "hpCon":
                        CostHp = node.GetShort();
                        break;
                    case "mpCon":
                        CostMp = node.GetShort();
                        break;
                    case "pdd":
                        WeaponDefense = node.GetShort();
                        break;
                    case "mdd":
                        MagicDefense = node.GetShort();
                        break;
                    case "pad":
                        WeaponAttack = node.GetShort();
                        break;
                    case "mad":
                        MagicAttack = node.GetShort();
                        break;
                    case "time":
                        BuffTime = node.GetInt();
                        break;
                    case "damage":
                        Damage = node.GetShort();
                        break;
                    case "range":
                        Range = node.GetShort();
                        break;
                    case "mobCount":
                        MobCount = (sbyte)node.GetShort();
                        break;
                    case "attackCount":
                        HitCount = (sbyte)node.GetShort();
                        break;
                    case "lt":
                        Lt = node.GetPoint();
                        break;
                    case "rb":
                        Rb = node.GetPoint();
                        break;
                    case "mastery":
                        Mastery = (sbyte)node.GetShort();
                        break;
                    case "speed":
                        Speed = node.GetShort();
                        break;
                    case "itemCon":
                        CostItem = node.GetInt();
                        break;
                    case "itemConNo":
                        ItemCount = node.GetShort();
                        break;
                    case "bulletCount":
                        CostBullet = node.GetShort();
                        break;
                    case "bulletConsume":
                        CostBullet = node.GetShort();
                        break;
                    case "jump":
                        Jump = node.GetShort();
                        break;
                    case "acc":
                        Accuracy = node.GetShort();
                        break;
                    case "eva":
                        Avoidability = node.GetShort();
                        break;
                    case "prop":
                        Probability = node.GetShort();
                        break;
                    case "moneyCon":
                        CostMeso = node.GetShort();
                        break;
                    case "hp":
                        Hp = node.GetShort();
                        break;
                    case "mp":
                        Mp = node.GetShort();
                        break;
                    default:
                        _log.Warning(
                            $"Unknown skill node Skill={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
