﻿using System.Collections.ObjectModel;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkills : Collection<MobSkill>
    {
        public Mob Parent { get; }

        public MobSkills(Mob parent)
        {
            Parent = parent;
        }

        public MobSkill Random => base[Functions.Random(Count - 1)];

        public new MobSkill this[int mapleId]
        {
            get
            {
                foreach (var loopMobSkill in this)
                {
                    if (loopMobSkill.MapleId == mapleId)
                    {
                        return loopMobSkill;
                    }
                }

                return null;
            }
        }

        public bool Contains(int mapleId, byte level)
        {
            foreach (var loopMobSkill in this)
            {
                if (loopMobSkill.MapleId == mapleId && loopMobSkill.Level == level)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
