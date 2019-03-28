using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSkill)]
    public class UseSkillHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var skillId = packet.ReadInt();
            var skillLevel = packet.ReadByte();
            var skill = client.Character.Skills[skillId];

            if (skill.CurrentLevel != skillLevel)
            {
                return;
            }

            if (!client.Character.IsAlive)
            {
                return;
            }

            skill.Character.Health -= skill.CostHp;
            skill.Character.Mana -= skill.CostMp;

            // TODO: Money cost.

            byte type = 0;
            short addedInfo = 0;

            switch (skill.MapleId)
            {
                case (int)SkillNames.Priest.MysticDoor:

                    HandleMysticDoor(packet);
                    break;

                case (int)SkillNames.Crusader.ArmorCrash:
                case (int)SkillNames.WhiteKnight.MagicCrash:
                case (int)SkillNames.DragonKnight.PowerCrash:
                {
                    HandleMobCrash(packet, skill);
                }
                    break;

                case (int)SkillNames.FirePoisonWizard.Slow:
                case (int)SkillNames.IceLightningWizard.Slow:
                case (int)SkillNames.Page.Threaten:
                {
                    packet.ReadInt(); // NOTE: Unknown, probably CRC.

                    var mobs = packet.ReadByte();

                    for (byte i = 0; i < mobs; i++)
                    {
                        var objectId = packet.ReadInt();

                        var mob = skill.Character.Map.Mobs[objectId];
                    }

                    // TODO: Apply mob status.
                }
                    break;

                case (int)SkillNames.FirePoisonMage.Seal:
                case (int)SkillNames.IceLightningMage.Seal:
                case (int)SkillNames.Priest.Doom:
                case (int)SkillNames.Hermit.ShadowWeb:
                {
                    var mobs = packet.ReadByte();

                    for (byte i = 0; i < mobs; i++)
                    {
                        var objectId = packet.ReadInt();

                        var mob = skill.Character.Map.Mobs[objectId];
                    }

                    // TODO: Apply mob status.
                }
                    break;

                case (int)SkillNames.Priest.Dispel:
                {
                }
                    break;

                case (int)SkillNames.Cleric.Heal:
                    HandleHeal(skill);
                    break;

                case (int)SkillNames.Fighter.Rage:
                case (int)SkillNames.Spearman.IronWill:
                case (int)SkillNames.Spearman.HyperBody:
                case (int)SkillNames.FirePoisonWizard.Meditation:
                case (int)SkillNames.IceLightningWizard.Meditation:
                case (int)SkillNames.Cleric.Bless:
                case (int)SkillNames.Priest.HolySymbol:
                case (int)SkillNames.Assassin.Haste:
                case (int)SkillNames.Hermit.MesoUp:
                case (int)SkillNames.Bandit.Haste:
                {
                    if (skill.Character.Party != null)
                    {
                        var targets = packet.ReadByte();

                        // TODO: Get affected party members.

                        var affected = new List<PartyMember>();

                        foreach (var member in affected)
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
                            {
                                oPacket.WriteByte((byte)UserEffect.SkillAffected);
                                oPacket.WriteInt(skill.MapleId);
                                oPacket.WriteByte(1);
                                oPacket.WriteByte(1);

                                member.Character.Client.Send(oPacket);
                            }

                            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerEffect))
                            {
                                oPacket.WriteInt(member.Character.Id);
                                oPacket.WriteByte((byte)UserEffect.SkillAffected);
                                oPacket.WriteInt(skill.MapleId);
                                oPacket.WriteByte(1);
                                oPacket.WriteByte(1);

                                member.Character.Map.Send(oPacket, member.Character);
                            }

                            member.Character.Buffs.Add(skill, 0);
                        }
                    }
                }
                    break;
                default:
                {
                    type = packet.ReadByte();

                    switch (type)
                    {
                        case 0x80:
                            addedInfo = packet.ReadShort();
                            break;
                    }
                }
                    break;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
            {
                oPacket.WriteByte((byte)UserEffect.SkillUse);
                oPacket.WriteInt(skill.MapleId);
                oPacket.WriteByte(1);
                oPacket.WriteByte(1);
                skill.Character.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerEffect))
            {
                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteByte((byte)UserEffect.SkillUse);
                oPacket.WriteInt(skill.MapleId);
                oPacket.WriteByte(1);
                oPacket.WriteByte(1);
                skill.Character.Map.Send(oPacket, skill.Character);
            }

            if (skill.HasBuff)
            {
                skill.Character.Buffs.Add(skill, 0);
            }
        }

        private static void HandleMysticDoor(PacketReader packet)
        {
            var origin = packet.ReadPoint();
            // NOTe: Prevents the default case from executing, there's no packet data left for it.
        }

        private static void HandleHeal(Skill skill)
        {
            var healthRate = skill.Hp;

            if (healthRate > 100)
            {
                healthRate = 100;
            }

            var partyPlayers = skill.Character.Party?.Count ?? 1;
            var healthMod = (short)(healthRate * skill.Character.MaxHealth / 100 / partyPlayers);

            if (skill.Character.Party != null)
            {
                var experience = 0;

                var members = skill.Character.Party.Values
                    .Where(member => member.Character != null)
                    .Where(member => member.Character.Map.MapleId == skill.Character.Map.MapleId)
                    .ToList();

                foreach (var member in members)
                {
                    var memberHealth = member.Character.Health;

                    if (memberHealth > 0 && memberHealth < member.Character.MaxHealth)
                    {
                        member.Character.Health += healthMod;

                        if (member.Character != skill.Character)
                        {
                            experience += 20 * (member.Character.Health - memberHealth) /
                                          (8 * member.Character.Level + 190);
                        }
                    }
                }

                if (experience > 0)
                {
                    skill.Character.Experience += experience;
                }
            }
            else
            {
                skill.Character.Health += healthRate;
            }
        }

        private static void HandleMobCrash(PacketReader packet, Skill skill)
        {
            packet.ReadInt(); // NOTE: Unknown, probably CRC.
            var mobs = packet.ReadByte();

            for (byte i = 0; i < mobs; i++)
            {
                var objectId = packet.ReadInt();
                var mob = skill.Character.Map.Mobs[objectId];
                // TODO: Mob crash skill.
            }
        }

        private static void HandleMist(PacketReader packet)
        {
            var origin = packet.ReadPoint();
        }

        private static void HandleMonsterMagnet(PacketReader packet, Character character, Skill skill)
        {
            var mobs = packet.ReadInt();

            for (var i = 0; i < mobs; i++)
            {
                var objectId = packet.ReadInt();
                var mob = skill.Character.Map.Mobs[objectId];
                var success = packet.ReadBool();
                mob?.SwitchController(character);
            }

            var direction = packet.ReadByte();
            //player.getMap().broadcastMessage(player, MaplePacketCreator.showBuffeffect(player.getId(), skillId, 1, direction), false);
            //c.getSession().write(MaplePacketCreator.enableActions());
        }
    }
}
