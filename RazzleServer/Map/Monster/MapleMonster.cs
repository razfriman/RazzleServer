using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Player;

namespace RazzleServer.Map.Monster
{
    public class MapleMonster
    {
        public bool Alive { get; internal set; }
        public int ObjectID { get; internal set; }

        internal void Damage(MapleCharacter chr, int damage)
        {
            throw new NotImplementedException();
        }

        internal void ApplyStatusEffect(int skillId, BuffStat sTUN, int v, int stunTime, MapleCharacter chr)
        {
            throw new NotImplementedException();
        }
    }
}
