using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MobSkillDataReference
    {
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
            if (int.TryParse(img.Name, out _))
            {
                Duration = img["time"]?.GetInt() ?? 0;
                MpCost = img["mpCon"]?.GetShort() ?? 0;
                ParameterA = img["x"]?.GetInt() ?? 0;
                ParameterB = img["y"]?.GetInt() ?? 0;
                Chance = img["prop"]?.GetShort() ?? 0;
                TargetCount = img["count"]?.GetShort() ?? 0;
                Cooldown = img["interval"]?.GetShort() ?? 0;
                Lt = img["lt"]?.GetPoint();
                Rb = img["rb"]?.GetPoint();
                PercentageLimitHp = img["hp"]?.GetShort() ?? 0;
                SummonLimit = img["limit"]?.GetShort() ?? 0;
                SummonEffect = img["summonEffect"]?.GetShort() ?? 0;

                var i = 0;
                while (img[i.ToString()] != null)
                {
                    Summons.Add(img[i.ToString()].GetInt());
                    i++;
                }
            }
        }
    }
}
