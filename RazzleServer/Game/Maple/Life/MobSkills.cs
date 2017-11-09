using System.Collections.ObjectModel;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkills : Collection<MobSkill>
    {
        public Mob Parent { get; private set; }

        public MobSkills(Mob parent)
        {
            Parent = parent;
        }

        public MobSkill Random => base[Functions.Random(this.Count - 1)];

        public new MobSkill this[int mapleID]
        {
            get
            {
                foreach (MobSkill loopMobSkill in this)
                {
                    if (loopMobSkill.MapleID == mapleID)
                    {
                        return loopMobSkill;
                    }
                }

                return null;
            }
        }

        public bool Contains(int mapleID, byte level)
        {
            foreach (MobSkill loopMobSkill in this)
            {
                if (loopMobSkill.MapleID == mapleID && loopMobSkill.Level == level)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
