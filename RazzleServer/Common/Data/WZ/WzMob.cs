using System.Collections.Generic;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Data.WZ
{
    public class WzMob
    {
        public int MobId { get; set; }
        public int Level { get; set; }
        public int PAD { get; set; }
        public int MAD { get; set; }
        public int PDD { get; set; }
        public int MDD { get; set; }
        public int PDRate { get; set; }
        public int MDRate { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int Acc { get; set; }
        public int Eva { get; set; }
        public int Speed { get; set; }
        public int Kb { get; set; }
        public int Exp { get; set; }
        public int Invincible { get; set; }
        public int FixedDamage { get; set; }
        public int SummonType { get; set; }                
        public string Name { get; set; }
        public bool FFALoot { get; set; }
        public bool ExplosiveReward { get; set; }
        public bool IsBoss { get; set; }
        public List<MobSkill> Skills = new List<MobSkill>();

        public byte MesoDrops
        {
            get
            {
                //TODO: this:
                /*if (getRemoveAfter() != 0 || isInvincible() || getOnlyNoramlAttack() || getDropItemPeriod() > 0 || getCP() > 0 || getPoint() > 0 || getFixedDamage() > 0 || getSelfD() != -1 || getPDRate() <= 0 || getMDRate() <= 0) {
                    return 0;*/
                int baseId = MobId / 100000;
                if (/*GameConstants.getPartyPlayHP(getId()) > 0 ||*/ baseId == 97 || baseId == 95 || baseId == 93 || baseId == 91 || baseId == 90)                
                    return 0;
                
                if (ExplosiveReward) {
                    return 7;
                }
                if (IsBoss) {
                    return 2;
                }
                return 1;                    
            }   
        }
    }
}
