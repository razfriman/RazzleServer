using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Skills;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterSummons : MapleKeyedCollection<int, Summon>
    {
        public Character Parent { get; set; }

        public CharacterSummons(Character parent) => Parent = parent;

        public override int GetKey(Summon item) => item.MapleId;


        public override void Remove(int key)
        {
            base.Remove(key);
            Parent.Map.Summons.Remove(key);
        }

        public void Add(Skill skill, Point position)
        {
            switch (skill.MapleId)
            {
                case (int)SkillNames.Priest.SummonDragon:
                case (int)SkillNames.Ranger.SilverHawk:
                case (int)SkillNames.Sniper.GoldenEagle:
                    Add(new Summon(Parent, skill, position, true) {Expiration = skill.Expiration});
                    break;

                case (int)SkillNames.Ranger.Puppet:
                case (int)SkillNames.Sniper.Puppet:
                    Add(new Puppet(Parent, skill, position, true) {Expiration = skill.Expiration});
                    break;
            }
        }

        public override void Add(Summon item)
        {
            base.Add(item);
            item.ScheduleExpiration();
            Parent.Map.Summons.Add(item);
        }

        public void RemovePuppet()
        {
            Remove((int)SkillNames.Ranger.Puppet);
            Remove((int)SkillNames.Sniper.Puppet);
        }

        public void MigrateSummons(Map oldField, Map newField)
        {
            foreach (var summon in Values)
            {
                oldField.Summons.Remove(summon);
                if (!(summon is Puppet))
                {
                    newField.Summons.Add(summon);
                }
            }
        }

        public void RemoveAll() => Values.ToList().ForEach(Remove);
    }
}
