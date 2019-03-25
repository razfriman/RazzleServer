using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class Attack
    {
        public AttackType Type { get; }
        public byte Portals { get; }
        public int Targets { get; }
        public int Hits { get; }
        public int SkillId { get; }
        public byte Display { get; }
        public byte Animation { get; }
        public byte WeaponClass { get; }
        public byte WeaponSpeed { get; }
        public int Ticks { get; }

        public uint TotalDamage { get; }

        public bool IsMesoExplosion { get; }

        public short StarPosition { get; }

        public List<Point> Positions { get; } = new List<Point>();
        public Dictionary<int, List<uint>> Damages { get; }

        public Attack(PacketReader packet, AttackType type)
        {
            Type = type;
            var tByte = packet.ReadByte();
            Targets = tByte / 0x10;
            Hits = tByte % 0x10;
            SkillId = packet.ReadInt();

            if (SkillId == (int)SkillNames.ChiefBandit.MesoExplosion)
            {
                IsMesoExplosion = true;
            }

            Portals = packet.ReadByte(); // Might be wrong 
            Display = packet.ReadByte();
            Animation = packet.ReadByte();
            //WeaponClass = packet.ReadByte();
            //WeaponSpeed = packet.ReadByte();
            //Ticks = packet.ReadInt();

            if (Type == AttackType.Range)
            {
                StarPosition = packet.ReadShort();
                packet.ReadByte(); // NOTE: Unknown.
            }

            Damages = new Dictionary<int, List<uint>>();

            for (var i = 0; i < Targets; i++)
            {
                var objectId = packet.ReadInt();
                packet.ReadInt(); // NOTE: Unknown.
                Positions.Add(packet.ReadPoint());
                packet.ReadPoint(); // NOTE: Damage position.

                if (Type == AttackType.Summon)
                {
                    packet.ReadByte();
                }
                else if (!IsMesoExplosion)
                {
                    packet.ReadShort(); // NOTE: Distance.
                }

                for (var j = 0; j < Hits; j++)
                {
                    var damage = packet.ReadUInt();

                    if (!Damages.ContainsKey(objectId))
                    {
                        Damages.Add(objectId, new List<uint>());
                    }

                    Damages[objectId].Add(damage);

                    TotalDamage += damage;
                }
            }

            if (Type == AttackType.Range)
            {
                var projectilePosition = packet.ReadPoint();
            }

            var playerPosition = packet.ReadPoint();
        }
    }
}
