using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.TakeDamage)]
    public class TakeDamageHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var attack = (sbyte)packet.ReadByte();
            var damage = packet.ReadInt();
            var reducedDamage = damage;
            var actualHpEffect = -damage;
            var actualMpEffect = 0;
            var healSkillId = 0;
            var mobSkillId = 0;
            var mobSkillLevel = (byte)0;
            Mob mob = null;

            if (attack <= -2)
            {
                // Mob Skill
                mobSkillLevel = packet.ReadByte();
                mobSkillId = packet.ReadByte();
            }
            else
            {
                var element = 0;
                var hasElement = packet.ReadBool();
                if (hasElement)
                {
                    element = packet.ReadInt();
                }

                var mobObjectId = packet.ReadInt();
                var mobId = packet.ReadInt();

                if (!client.Character.Map.Mobs.Contains(mobObjectId))
                {
                    return;
                }

                mob = client.Character.Map.Mobs[mobObjectId];

                if (mobId != mob.MapleId)
                {
                    return;
                }

                var stance = packet.ReadByte();
                var isReflected = packet.ReadBool();

                var reflectHitAction = (byte)0;
                var reflectPosition = new Point(0, 0);

                if (isReflected)
                {
                    reflectHitAction = packet.ReadByte();
                    reflectPosition = packet.ReadPoint();
                }

                if (client.Character.PrimaryStats.HasBuff((int)SkillNames.Magician.MagicGuard) &&
                    client.Character.PrimaryStats.Mana > 0)
                {
                    // Absorb damage
                    //  TODO
                }

                // TODO - PowerGuard
                // TODO - MesoGuard

                SendDamage(
                    client.Character,
                    attack,
                    damage,
                    reducedDamage,
                    healSkillId,
                    mobObjectId,
                    mobId,
                    stance,
                    isReflected,
                    reflectHitAction,
                    reflectPosition
                );
            }

            if (actualHpEffect < 0)
            {
                client.Character.PrimaryStats.Health += (short)actualHpEffect;
            }

            if (actualMpEffect < 0)
            {
                client.Character.PrimaryStats.Mana += (short)actualMpEffect;
            }

            if (mobSkillLevel != 0 && mobSkillId != 0)
            {
                // Check if the skill exists and has any extra effect.
                if (!DataProvider.MobSkills.Data.ContainsKey(mobSkillId) ||
                    !DataProvider.MobSkills.Data[mobSkillId].ContainsKey(mobSkillLevel))
                {
                    return;
                }

                var skill = DataProvider.MobSkills.Data[mobSkillId][mobSkillLevel];
                OnStatChangeByMobSkill(client.Character, skill);
            }
            else if (mob != null)
            {
                if (!mob.CachedReference.Attacks.TryGetValue((byte)attack, out var mobAttack))
                {
                    return;
                }

                if (mobAttack.Disease <= 0)
                {
                    return;
                }

                if (!DataProvider.MobSkills.Data.ContainsKey(mobAttack.Disease) ||
                    !DataProvider.MobSkills.Data[mobAttack.Disease].ContainsKey(mobAttack.SkillLevel))
                {
                    return;
                }

                var diseaseSkill = DataProvider.MobSkills.Data[mobSkillId][mobSkillLevel];
                OnStatChangeByMobSkill(client.Character, diseaseSkill);
            }
        }

        public static void OnStatChangeByMobSkill(Character chr, MobSkillDataReference mobSkill, short delay = 0)
        {
            // See if we can actually set the effect...
//            int prop = 100;
//            if (mobSkill.Prop != 0)
//                prop = mobSkill.Prop;
//
//            if (Rand32.Next() % 100 >= prop) return; // Luck.
//
//            BuffStat setStat = null;
//            int rValue = mobSkill.SkillID | (mobSkill.Level << 16);
//            var ps = chr.PrimaryStats;
//            int nValue = 1;
//            switch ((Constants.MobSkills.Skills)mobSkill.SkillID)
//            {
//                case Constants.MobSkills.Skills.Seal: setStat = ps.BuffSeal; break;
//                case Constants.MobSkills.Skills.Darkness: setStat = ps.BuffDarkness; break;
//                case Constants.MobSkills.Skills.Weakness: setStat = ps.BuffWeakness; break;
//                case Constants.MobSkills.Skills.Stun: setStat = ps.BuffStun; break;
//                case Constants.MobSkills.Skills.Curse: setStat = ps.BuffCurse; break;
//                case Constants.MobSkills.Skills.Poison:
//                    setStat = ps.BuffPoison;
//                    nValue = mobSkill.X;
//                    break;
//            }
//
//            if (setStat != null && !setStat.IsSet())
//            {
//                var buffTime = mobSkill.Time * 1000;
//                var stat = setStat.Set(rValue, (short)nValue, BuffStat.GetTimeForBuff(buffTime + delay));
//
//                if (stat != 0)
//                {
//                    chr.Buffs.FinalizeBuff(stat, delay);
//                }
//            }
        }

        public static void SendDamage(Character chr,
            sbyte attacType,
            int initialDamage,
            int reducedDamage,
            int healSkillId,
            int mobObjectId,
            int mobId,
            byte stance,
            bool isReflected,
            byte reflectHitAction,
            Point reflectPosition)
        {
            var pw = new PacketWriter(ServerOperationCode.RemotePlayerGetDamage);
            pw.WriteInt(chr.Id);
            pw.WriteSByte(attacType);
            pw.WriteInt(initialDamage);

            pw.WriteInt(mobObjectId);
            pw.WriteInt(mobId);
            pw.WriteByte(stance);
            pw.WriteBool(isReflected);

            if (isReflected)
            {
                pw.WriteByte(reflectHitAction);
                pw.WritePoint(reflectPosition);
            }

            pw.WriteInt(reducedDamage);
            if (reducedDamage < 0)
            {
                pw.WriteInt(healSkillId);
            }

            chr.Map.Send(pw, chr);
        }
    }
}
