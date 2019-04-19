using System.Collections.ObjectModel;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkills : MapleKeyedCollection<int, MobSkill>
    {
        public Mob Parent { get; }

        public MobSkills(Mob parent) => Parent = parent;

        public MobSkill Random => base[Functions.Random(Count - 1)];

        public override int GetKey(MobSkill item) => item.MapleId;
    }
}
