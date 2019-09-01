using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Life
{
    public class Mist : IMapObject, ISpawnable
    {
        private Rectangle Bounds { get; set; }
        public GameCharacter Owner { get; set; }
        public Skill Skill { get; set; }
        public MistType MistType { get; set; }
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }

        public Mist(Rectangle boundingBox, GameCharacter gameCharacter, Skill skill)
        {
            Skill = skill;
            MistType = CalculateMistType();
            Owner = gameCharacter;
            Bounds = boundingBox;
        }

        public MistType CalculateMistType()
        {
            return Skill.MapleId switch
            {
                (int)SkillNames.FirePoisonMage.PoisonMist => MistType.Poison,
                _ => MistType.Mob
            };
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket() => GetInternalPacket();

        private PacketWriter GetInternalPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.MistEnterField);
            pw.WriteInt(ObjectId);
            pw.WriteInt((int)MistType);
            pw.WriteInt(Owner.Id);
            pw.WriteInt(Skill.MapleId);
            pw.WriteByte(Skill.CurrentLevel);
            pw.WriteShort(0); // Cooldown
            pw.WriteInt(Bounds.Rb.X);
            pw.WriteInt(Bounds.Rb.Y);
            pw.WriteInt(Bounds.Rb.X + Bounds.Lt.Y);
            pw.WriteInt(Bounds.Rb.Y + Bounds.Lt.Y);
            pw.WriteInt(0);
            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.MistLeaveField);
            pw.WriteInt(ObjectId);
            return pw;
        }
    }
}
