using System.Collections.Generic;

namespace RazzleServer.Player
{
    public class AttackPair
    {
        public int TargetObjectID { get; set; }
        public Point Position { get; set; }
        public List<int> Damages { get; set; }
        public List<bool> Criticals { get; set; }
        public AttackPair()
        {
            Damages = new List<int>();
            Criticals = new List<bool>();
        }
    }
}
