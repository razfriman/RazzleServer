using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MobSkillReference
    {
        public byte MapleId { get; set; }
        public byte Level { get; set; }
        public byte Action { get; set; }
        public short EffectDelay { get; set; }


        public MobSkillReference()
        {
        }

        public MobSkillReference(WzImageProperty img)
        {
            MapleId = (byte)(img["skill"]?.GetInt() ?? 0);
            Level = (byte)(img["level"]?.GetInt() ?? 0);
            Action = (byte)(img["action"]?.GetInt() ?? 0);
            EffectDelay = img["effectAfter"]?.GetShort() ?? 0;
        }
    }
}
