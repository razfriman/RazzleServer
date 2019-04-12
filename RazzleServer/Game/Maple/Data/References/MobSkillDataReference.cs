using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MobSkillDataReference
    {
        private readonly ILogger _log = Log.ForContext<SkillReference>();

        public List<int> Summons { get; set; } = new List<int>();
        public int Duration { get; private set; }
        public short MpCost { get; private set; }
        public int ParameterA { get; private set; }
        public int ParameterB { get; private set; }
        public short Chance { get; private set; }
        public short TargetCount { get; private set; }
        public int Cooldown { get; private set; }
        public Point? Lt { get; private set; }
        public Point? Rb { get; private set; }
        public short PercentageLimitHp { get; private set; }
        public short SummonLimit { get; private set; }
        public short SummonEffect { get; private set; }

        public MobSkillDataReference()
        {
        }

        public MobSkillDataReference(WzImageProperty img)
        {
            if (!int.TryParse(img.Name, out var id))
            {
                return;
            }

            foreach (var node in img.WzProperties)
            {
                switch (node.Name)
                {
                    case "affected":
                    case "effect":
                    case "mob":
                    case "mob0":
                    case "tile":
                        break;
                    case "time":
                        Duration = node.GetShort();
                        break;
                    case "mpCon":
                        MpCost = node.GetShort();
                        break;
                    case "x":
                        ParameterA = node.GetInt();
                        break;
                    case "y":
                        ParameterB = node.GetInt();
                        break;
                    case "prop":
                        Chance = node.GetShort();
                        break;
                    case "count":
                        TargetCount = node.GetShort();
                        break;
                    case "interval":
                        Cooldown = node.GetShort();
                        break;
                    case "lt":
                        Lt = node.GetPoint();
                        break;
                    case "rb":
                        Rb = node.GetPoint();
                        break;
                    case "hp":
                        PercentageLimitHp = node.GetShort();
                        break;
                    case "limit":
                        SummonLimit = node.GetShort();
                        break;
                    case "summonEffect":
                        SummonEffect = node.GetShort();
                        break;
                    case string summonId when int.TryParse(summonId, out _):
                        Summons.Add(node.GetInt());
                        break;
                    default:
                        _log.Warning(
                            $"Unknown mob skill data node Skill={id} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
