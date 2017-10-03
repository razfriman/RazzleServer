using RazzleServer.Packet;
using System.Collections.Generic;
using System.Drawing;

namespace RazzleServer.Movement
{
    public abstract class MapleMovementFragment
    {
        public byte Type { get; set; }
        public Point Position { get; set; }
        public short Duration { get; set; }
        public byte State { get; set; }
        private short fh = 0;
        public short Foothold { get { return fh; } set { fh = value; } }

        public MapleMovementFragment(byte type, Point position, byte state, short duration)
        {
            Type = type;
            Position = position;
            Duration = duration;
            State = state;
        }

        public abstract void Serialize(PacketWriter pw);

        public static void WriteMovementList(PacketWriter pw, List<MapleMovementFragment> movementList)
        {            
            pw.WriteByte((byte)movementList.Count);
            foreach (MapleMovementFragment mmf in movementList)
            {                
                mmf.Serialize(pw);
            }
        }
    }
}