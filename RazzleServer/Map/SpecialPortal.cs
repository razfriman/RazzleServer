using RazzleServer.Data.WZ;
using RazzleServer.Packet;
using RazzleServer.Player;
using System.Drawing;
using static RazzleServer.Data.WZ.WzMap;
using MapleLib.PacketLib;

namespace RazzleServer.Map
{
    public class SpecialPortal : StaticMapObject
    {
        public int SkillId { get; private set; }
        public MapleMap FromMap { get; set; }
        public MapleMap ToMap { get; set; }
        public Portal ToMapPortal { get; set; }

        public SpecialPortal(int skillId, MapleCharacter owner, Point position, MapleMap fromMap, MapleMap toMap, WzMap.Portal toMapSpawnPortal, int durationMS, bool partyObject)
            : base(0, owner, position, durationMS, partyObject)
        {
            SkillId = skillId;
            FromMap = fromMap;
            ToMap = toMap;
            ToMapPortal = toMapSpawnPortal;
        }

        public override void Dispose()
        {
            Owner.RemoveDoor(SkillId);
            FromMap = null;
            ToMapPortal = null;
            base.Dispose();
        }

        public void Warp(MapleCharacter chr) => chr.ChangeMap(ToMap, ToMapPortal.Name, true);

        public override PacketWriter GetSpawnPacket(bool animatedSpawn)
        {
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SPAWN_PORTAL);
            pw.WriteInt(ToMap.MapID);
            pw.WriteInt(FromMap.MapID);
            pw.WriteInt(SkillId);
            pw.WritePoint(Position);
            return pw;
        }

        public override PacketWriter GetDestroyPacket(bool animatedDestroy)
        {
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SPAWN_PORTAL);
            pw.WriteInt(999999999);
            pw.WriteInt(999999999);
            return pw;
        }
    }

    public class MysticDoor : SpecialPortal
    {
        public MysticDoor(int skillId, MapleCharacter owner, Point position, MapleMap fromMap, MapleMap toMap, Portal toMapSpawnPortal, int durationMS, bool partyObject)
            : base(skillId, owner, position, fromMap, toMap, toMapSpawnPortal, durationMS, partyObject)
        {

        }

        public override PacketWriter GetSpawnPacket(bool animatedSpawn)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SPAWN_SPECIAL_MAPOBJECT);
            pw.WriteBool(!animatedSpawn);
            pw.WriteInt(Owner.ID);
            pw.WriteInt(SkillId);
            pw.WritePoint(Position);

            return pw;
        }

        public override PacketWriter GetDestroyPacket(bool animatedDestroy)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.REMOVE_SPECIAL_MAPOBJECT);
            pw.WriteBool(true);
            pw.WriteInt(Owner.ID);
            return pw;
        }
    }
}
