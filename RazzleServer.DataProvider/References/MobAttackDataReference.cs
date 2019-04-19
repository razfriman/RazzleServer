using RazzleServer.Common.Util;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    public class MobAttackDataReference
    {
        private readonly ILogger _log = Log.ForContext<MobAttackDataReference>();

        public byte Id { get; set; }
        public short MpConsume { get; set; }
        public short PaDamage { get; set; }
        public byte Type { get; set; }
        public string ElemAttr { get; set; }
        public Point RangeLt { get; set; }
        public Point RangeRb { get; set; }
        public short RangeR { get; set; }
        public Point RangeSp { get; set; }

        public bool Magic { get; set; }

        /// <summary>
        /// Referred to as "disease" in the data files
        /// </summary>
        public byte SkillId { get; set; }

        public byte SkillLevel { get; set; }

        public MobAttackDataReference()
        {
        }

        public MobAttackDataReference(WzImageProperty attackNode)
        {
            Id = byte.Parse(attackNode.Name[attackNode.Name.Length - 1].ToString());

            foreach (var node in attackNode["info"].WzPropertiesList)
            {
                switch (node.Name)
                {
                    // Effects
                    case "ball":
                    case "hit":
                    case "bulletSpeed":
                    case "attackAfter":
                    case "effect":
                    case "effectAfter":
                    case "jumpAttack":
                        break;

                    //TODO
                    case "tremble":
                    case "effect0":
                    case "areaWarning":
                    case "mpBurn":
                    case "doFirst":
                    case "deadlyAttack":
                        break;

                    case "disease":
                        SkillId = (byte)node.GetInt();
                        break;
                    case "elemAttr":
                        ElemAttr = node.GetString();
                        break;
                    case "conMP":
                        MpConsume = node.GetShort();
                        break;
                    case "magic":
                        Magic = node.GetInt() > 0;
                        break;
                    case "type":
                        Type = (byte)node.GetInt();
                        break;
                    case "PADamage":
                        PaDamage = node.GetShort();
                        break;
                    case "level":
                        SkillLevel = (byte)node.GetInt();
                        break;
                    case "range":
                        if (node["lt"] != null)
                        {
                            RangeLt = node["lt"].GetPoint();
                            RangeRb = node["rb"].GetPoint();
                        }
                        else
                        {
                            RangeR = node["r"].GetShort();
                            RangeSp = node["sp"].GetPoint();
                        }

                        break;
                    default:
                        _log.Warning(
                            $"Unknown mob attack node Parent={attackNode.Name} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
