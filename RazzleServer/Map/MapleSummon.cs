using RazzleServer.Constants;
using RazzleServer.Map.Monster;
using RazzleServer.Movement;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RazzleServer.Map
{
    public class MapleSummon
    {
        public int ObjectID { get; set; }
        public int SourceSkillId { get; private set; }
        public Point Position { get; set; }
        public int TickCount { get; set; }
        public MapleCharacter Owner { get; private set; }
        public byte SkillLevel { get; private set; }
        public int HP { get; private set; }
        public DateTime LastAbilityTime { get; set; }
        public SummonType Type { get; private set; }
        public SummonMovementType MovementType { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }

        public MapleSummon(int objectId, int sourceSkillId, Point position, SummonType type, SummonMovementType movementType, MapleCharacter owner, byte skillLevel, int durationMS)
        {
            ObjectID = objectId;
            SourceSkillId = sourceSkillId;
            Position = position;
            Owner = owner;
            SkillLevel = skillLevel;
            Type = type;
            MovementType = movementType;
            HP = 0;
            LastAbilityTime = DateTime.FromFileTime(0);
            if (durationMS > 0)
            {
                CancellationToken = new CancellationTokenSource();
                Scheduler.ScheduleRemoveSummon(owner, sourceSkillId, durationMS, CancellationToken.Token);
            }
        }

        public void Dispose()
        {
            CancellationToken.Cancel();
            Owner = null;
        }

        public void AttackMonster(int damage, byte speed, MapleMonster monster)
        {
            AttackInfo info = new AttackInfo();
            info.Attacks = 1;
            info.Targets = 1;
            info.AttacksByte = 0x11;
            info.Speed = speed;
            AttackPair attackPair = new AttackPair();
            attackPair.TargetObjectID = monster.ObjectID;
            attackPair.Damages = new List<int>() { damage };
            info.TargetDamageList = new List<AttackPair>() { attackPair };
            Owner.Map.BroadcastPacket(GetAttackPacket(info, true));
            monster.Damage(Owner, damage);
        }

        #region Packets
        public PacketWriter GetSpawnPacket(bool spawnAnimated)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SPAWN_SPECIAL_MAPOBJECT); // SPAWN_SUMMON

            pw.WriteInt(Owner.ID);
            pw.WriteInt(ObjectID);
            pw.WriteInt(SourceSkillId);
            pw.WriteByte(Owner.Level);
            pw.WriteByte(SkillLevel);
            pw.WritePoint(Position);
            if (SourceSkillId == BattleMage3.SUMMON_REAPER_BUFF || SourceSkillId == WildHunter2.CALL_OF_THE_WILD)
                pw.WriteByte(5);
            else
                pw.WriteByte(4);
            pw.WriteShort(0); //foothold
            pw.WriteByte((byte)MovementType);
            pw.WriteByte((byte)Type);
            pw.WriteBool(spawnAnimated); //true = new spawn, false = no spawn animation
            pw.WriteByte(1);
            if (SourceSkillId == DualBlade4.MIRRORED_TARGET)
            {
                pw.WriteByte(1);
                MapleCharacter.AddCharLook(pw, Owner, true);
            }
            else
                pw.WriteByte(0);
            if (SourceSkillId == Mechanic3.ROCK_N_SHOCK)
                pw.WriteByte(0);
            if (SourceSkillId == Kanna3.KISHIN_SHOUKAN)
                pw.WriteZeroBytes(8);
            return pw;
        }

        public PacketWriter RemovePacket(bool animated)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.REMOVE_SPECIAL_MAPOBJECT); // REMOVE_SUMMON

            pw.WriteInt(Owner.ID);
            pw.WriteInt(ObjectID);
            if (animated)
                pw.WriteByte(4);
            else
                pw.WriteByte(1);

            return pw;
        }

        public PacketWriter MovePacket(Point startPosition, List<MapleMovementFragment> movementList)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.MOVE_SUMMON);

            pw.WriteInt(Owner.ID);
            pw.WriteInt(ObjectID);
            pw.WriteInt(0);
            pw.WritePoint(startPosition);
            pw.WriteInt(0);
            MapleMovementFragment.WriteMovementList(pw, movementList);

            return pw;

        }

        public PacketWriter GetUseSkillPacket(int skillId, byte stance)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SUMMON_SKILL);
            pw.WriteInt(Owner.ID);
            pw.WriteInt(skillId);
            pw.WriteByte(stance);
            return pw;
        }

        public PacketWriter GetAttackPacket(AttackInfo attackInfo, bool darkFlare = false)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.SUMMON_ATTACK);
            pw.WriteInt(Owner.ID);
            pw.WriteInt(ObjectID);
            pw.WriteByte(Owner.Level);
            pw.WriteByte(attackInfo.Speed);
            pw.WriteByte(attackInfo.AttacksByte);
            foreach (AttackPair ap in attackInfo.TargetDamageList)
            {
                pw.WriteInt(ap.TargetObjectID);
                pw.WriteByte(7);
                pw.WriteInt(ap.Damages.FirstOrDefault());
            }
            pw.WriteBool(darkFlare);
            return pw;
        }
        #endregion

        public bool IsPuppet
        {
            get
            {
                return SkillConstants.IsPuppetSummon(SourceSkillId);
            }
        }
    }

    public enum SummonMovementType : byte
    {
        Stationary = 0,
        Follow = 1,
        TeleportFollow = 3,
        WalkStationary = 2,
        CircleFollow = 4,
        CircleStationary = 5
    }

    public enum SummonType : byte
    {
        Passive = 0, //Does nothing
        Aggressive = 1, //Attacks whatever in range?
        Buff = 2,
        Assist = 3, //Attacks your target
        SubMinion = 5,
        Charge = 6,
        ReflectDamage = 7, //Attacks mobs that hit you
        ShikigamiCharm = 8
    }
}
