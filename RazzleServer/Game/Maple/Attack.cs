using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple
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
        public Dictionary<int, List<uint>> Damages { get; }

        public Attack(PacketReader packet, AttackType type)
        {
            Type = type;
            Portals = packet.ReadByte();
            var tByte = packet.ReadByte();
            Targets = tByte / 0x10;
            Hits = tByte % 0x10;
            SkillId = packet.ReadInt();



            switch (SkillId)
            {
                case 2121001:
                case 2221001:
                case 2321001:
                case 5101004:
                case 5201002:
                    var charge = packet.ReadInt();
                    break;
            }

            if (SkillId == 1221011)
            {
                //isHH = true;
            }

            if (SkillId == 4211006) 
            {
                // parseMesoExplosion(lea, ret);
            }

            Display = packet.ReadByte();
            Animation = packet.ReadByte();
            WeaponClass = packet.ReadByte();
            WeaponSpeed = packet.ReadByte();
            Ticks = packet.ReadInt();

            if (Type == AttackType.Range)
            {
                var starSlot = packet.ReadShort();
                var cashStarSlot = packet.ReadShort();
                packet.ReadByte(); // NOTE: Unknown.
            }

            // [29 00] [01] [11] [00 00 00 00]
            // [00] [85] [01] [04] [67 77 19 02]
            // [1A 00 00 00] [06 80 01 01] [61 00 12 01] [63 00 12 01] [89 01] [01 00 00 00] [CF 99 F9 DF] [9F 00 12 01]

            Damages = new Dictionary<int, List<uint>>();

            for (var i = 0; i < Targets; i++)
            {
                var objectId = packet.ReadInt();
                packet.ReadInt(); // NOTE: Unknown.
                packet.ReadPoint(); // NOTE: Mob position.
                packet.ReadPoint(); // NOTE: Damage position.
                packet.ReadShort(); // NOTE: Distance.

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

                if (Type != AttackType.Summon)
                {
                    packet.ReadInt(); // NOTE: Unknown, probably CRC.
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
