using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MobSkillReference
    {
        private readonly ILogger _log = Log.ForContext<MobSkillReference>();

        public byte MapleId { get; set; }
        public byte Level { get; set; }
        public byte Action { get; set; }
        public short EffectDelay { get; set; }

        public MobSkillReference()
        {
        }

        public MobSkillReference(WzImageProperty img)
        {
            foreach (var node in img.WzProperties)
            {
                switch (node.Name)
                {
                    case "skill":
                        MapleId = (byte)node.GetInt();
                        break;
                    case "level":
                        Level = (byte)node.GetInt();
                        break;
                    case "action":
                        Action = (byte)node.GetInt();
                        break;
                    case "effectAfter":
                        EffectDelay = node.GetShort();
                        break;
                    default:
                        _log.Warning(
                            $"Unknown mob skill node Skill={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }
        }
    }
}
