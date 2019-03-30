using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Life
{
    public class Summon : MapObject, ISpawnable
    {
        public Character Parent { get; set; }
        public int MapleId { get; set; }
        public byte Level { get; set; }
        public bool MoveAction { get; set; }
        public ushort Foothold { get; set; }
        public DateTime Expiration { get; set; } = DateConstants.Permanent;

        public Summon(Character parent, Skill skill, Point position, bool moveAction)
        {
            Parent = parent;
            MapleId = skill.MapleId;
            Level = skill.CurrentLevel;
            Position = position;
            MoveAction = moveAction;
        }

        public void ScheduleExpiration()
        {
            Delay.Execute(() =>
            {
                if (Parent.Summons.Contains(MapleId))
                {
                    Parent.Summons.Remove(this);
                }
            }, (int)(Expiration - DateTime.Now).TotalMilliseconds);
        }

        public PacketWriter GetCreatePacket()
        {
            throw new System.NotImplementedException();
        }

        public PacketWriter GetDestroyPacket()
        {
            throw new System.NotImplementedException();
        }

        public PacketWriter GetSpawnPacket()
        {
            throw new System.NotImplementedException();
        }
    }
}
