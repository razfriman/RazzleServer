using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Game.Maple.Util;

namespace RazzleServer.Game.Maple.Life
{
    public class Mist : MapObject, ISpawnable
    {
        private Rectangle Bounds { get; set; }
        public Character Owner { get; set; }
        public Skill Skill { get; set; }
        public MistType MistType { get; set; }

        public Mist(Rectangle boundingBox, Character character, Skill skill)
        {
            Skill = skill;
            MistType = CalculateMistType();
            Owner = character;
            Bounds = boundingBox;
        }

        public MistType CalculateMistType()
        {
            switch (Skill.MapleId)
            {
                case (int)SkillNames.FirePoisonMage.PoisonMist:
                    return MistType.Poison;
            }

            return MistType.Mob;
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket() => GetInternalPacket();

        private PacketWriter GetInternalPacket()
        {
            using (var pw = new PacketWriter(ServerOperationCode.MistEnterField))
            {
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
        }

        public PacketWriter GetDestroyPacket()
        {
            using (var pw = new PacketWriter(ServerOperationCode.MistLeaveField))
            {
                pw.WriteInt(ObjectId);
                return pw;
            }
        }

    }
}
