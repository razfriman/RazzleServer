using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Skills;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterRings : MapleKeyedCollection<int, Ring>
    {
        public Character Parent { get; }

        public CharacterRings(Character parent)
        {
            Parent = parent;
        }

        public byte[] ToByteArray()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteShort((short)Count);

                foreach (var ring in Values)
                {
                    pw.WriteInt(ring.Partner.Id);
                    pw.WriteString(ring.Partner.Name, 13);
                    pw.WriteInt(ring.MapleId);
                    pw.WriteInt(0);
                    pw.WriteInt(ring.PartnerRingId);
                    pw.WriteInt(0);
                }

                return pw.ToArray();
            }
        }

        public override int GetKey(Ring ring) => ring.MapleId;

        public void Save()
        {
        }
    }
}
