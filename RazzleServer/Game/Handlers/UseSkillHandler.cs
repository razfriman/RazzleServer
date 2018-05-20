using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseSkill)]
    public class UseSkillHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            // TODO - WHICH SKILL?

            var skill = client.Character.Skills[0];


            if (!client.Character.IsAlive)
            {
                return;
            }

            if (skill.IsCoolingDown)
            {
                return;
            }

            if (skill.MapleId == (int)SkillNames.Priest.MysticDoor)
            {
                var origin = packet.ReadPoint();
                // TODO: Open mystic door.
            }

            skill.Character.Health -= skill.CostHP;
            skill.Character.Mana -= skill.CostMP;

            if (skill.Cooldown > 0)
            {
                skill.CooldownEnd = DateTime.Now.AddSeconds(skill.Cooldown);
            }

            // TODO: Money cost.

            byte type = 0;
            byte direction = 0;
            short addedInfo = 0;

            switch (skill.MapleId)
            {
                case (int)SkillNames.Priest.MysticDoor:
                    // NOTe: Prevents the default case from executing, there's no packet data left for it.
                    break;

                case (int)SkillNames.Shadower.Smokescreen:
                    {
                        var origin = packet.ReadPoint();
                        // TODO: Mists.
                    }
                    break;

                case (int)SkillNames.Crusader.ArmorCrash:
                case (int)SkillNames.WhiteKnight.MagicCrash:
                case (int)SkillNames.DragonKnight.PowerCrash:
                    {
                        packet.ReadInt(); // NOTE: Unknown, probably CRC.
                        byte mobs = packet.ReadByte();

                        for (byte i = 0; i < mobs; i++)
                        {
                            int objectId = packet.ReadInt();

                            var mob = skill.Character.Map.Mobs[objectId];

                            // TODO: Mob crash skill.
                        }
                    }
                    break;

                case (int)SkillNames.Hero.MonsterMagnet:
                case (int)SkillNames.Paladin.MonsterMagnet:
                case (int)SkillNames.DarkKnight.MonsterMagnet:
                    {
                        int mobs = packet.ReadInt();

                        for (int i = 0; i < mobs; i++)
                        {
                            int objectId = packet.ReadInt();

                            var mob = skill.Character.Map.Mobs[objectId];

                            bool success = packet.ReadBool();

                            // TODO: Packet.
                        }

                        direction = packet.ReadByte();
                    }
                    break;

                case (int)SkillNames.FirePoisonWizard.Slow:
                case (int)SkillNames.IceLightningWizard.Slow:
                case (int)SkillNames.Page.Threaten:
                    {
                        packet.ReadInt(); // NOTE: Unknown, probably CRC.

                        byte mobs = packet.ReadByte();

                        for (byte i = 0; i < mobs; i++)
                        {
                            int objectId = packet.ReadInt();

                            var mob = skill.Character.Map.Mobs[objectId];
                        }

                        // TODO: Apply mob status.
                    }
                    break;

                case (int)SkillNames.FirePoisonMage.Seal:
                case (int)SkillNames.IceLightningMage.Seal:
                case (int)SkillNames.Priest.Doom:
                case (int)SkillNames.Hermit.ShadowWeb:
                case (int)SkillNames.Shadower.NinjaAmbush:
                case (int)SkillNames.NightLord.NinjaAmbush:
                    {
                        byte mobs = packet.ReadByte();

                        for (byte i = 0; i < mobs; i++)
                        {
                            int objectId = packet.ReadInt();

                            var mob = skill.Character.Map.Mobs[objectId];
                        }

                        // TODO: Apply mob status.
                    }
                    break;

                case (int)SkillNames.Bishop.HerosWill:
                case (int)SkillNames.IceLightningArchMage.HerosWill:
                case (int)SkillNames.FirePoisonArchMage.HerosWill:
                case (int)SkillNames.DarkKnight.HerosWill:
                case (int)SkillNames.Hero.HerosWill:
                case (int)SkillNames.Paladin.HerosWill:
                case (int)SkillNames.NightLord.HerosWill:
                case (int)SkillNames.Shadower.HerosWill:
                case (int)SkillNames.Bowmaster.HerosWill:
                case (int)SkillNames.Marksman.HerosWill:
                    {
                        // TODO: Add Buccaneer & Corsair.

                        // TODO: Remove Sedcude debuff.
                    }
                    break;

                case (int)SkillNames.Priest.Dispel:
                    {

                    }
                    break;

                case (int)SkillNames.Cleric.Heal:
                    {
                        short healthRate = skill.HP;

                        if (healthRate > 100)
                        {
                            healthRate = 100;
                        }

                        int partyPlayers = skill.Character.Party?.Count ?? 1;
                        short healthMod = (short)(((healthRate * skill.Character.MaxHealth) / 100) / partyPlayers);

                        if (skill.Character.Party != null)
                        {
                            int experience = 0;

                            var members = new List<PartyMember>();

                            foreach (var member in skill.Character.Party)
                            {
                                if (member.Character != null && member.Character.Map.MapleId == skill.Character.Map.MapleId)
                                {
                                    members.Add(member);
                                }
                            }

                            foreach (PartyMember member in members)
                            {
                                short memberHealth = member.Character.Health;

                                if (memberHealth > 0 && memberHealth < member.Character.MaxHealth)
                                {
                                    member.Character.Health += healthMod;

                                    if (member.Character != skill.Character)
                                    {
                                        experience += 20 * (member.Character.Health - memberHealth) / (8 * member.Character.Level + 190);
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
                    break;

                case (int)SkillNames.Fighter.Rage:
                case (int)SkillNames.Spearman.IronWill:
                case (int)SkillNames.Spearman.HyperBody:
                case (int)SkillNames.FirePoisonWizard.Meditation:
                case (int)SkillNames.IceLightningWizard.Meditation:
                case (int)SkillNames.Cleric.Bless:
                case (int)SkillNames.Priest.HolySymbol:
                case (int)SkillNames.Bishop.Resurrection:
                case (int)SkillNames.Bishop.HolyShield:
                case (int)SkillNames.Bowmaster.SharpEyes:
                case (int)SkillNames.Marksman.SharpEyes:
                case (int)SkillNames.Assassin.Haste:
                case (int)SkillNames.Hermit.MesoUp:
                case (int)SkillNames.Bandit.Haste:
                case (int)SkillNames.Hero.MapleWarrior:
                case (int)SkillNames.Paladin.MapleWarrior:
                case (int)SkillNames.DarkKnight.MapleWarrior:
                case (int)SkillNames.FirePoisonArchMage.MapleWarrior:
                case (int)SkillNames.IceLightningArchMage.MapleWarrior:
                case (int)SkillNames.Bishop.MapleWarrior:
                case (int)SkillNames.Bowmaster.MapleWarrior:
                case (int)SkillNames.Marksman.MapleWarrior:
                case (int)SkillNames.NightLord.MapleWarrior:
                case (int)SkillNames.Shadower.MapleWarrior:
                    {

                        if (skill.Character.Party != null)
                        {
                            byte targets = packet.ReadByte();

                            // TODO: Get affected party members.

                            List<PartyMember> affected = new List<PartyMember>();

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

                                using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
                                {
                                    oPacket.WriteInt(member.Character.Id);
                                    oPacket.WriteByte((byte)UserEffect.SkillAffected);
                                    oPacket.WriteInt(skill.MapleId);
                                    oPacket.WriteByte(1);
                                    oPacket.WriteByte(1);

                                    member.Character.Map.Broadcast(oPacket, member.Character);
                                }

                                member.Character.Buffs.Add(skill, 0);
                            }
                        }
                    }
                    break;

                case (int)SkillNames.Beginner.EchoOfHero:
                case (int)SkillNames.SuperGM.Haste:
                case (int)SkillNames.SuperGM.HolySymbol:
                case (int)SkillNames.SuperGM.Bless:
                case (int)SkillNames.SuperGM.HealPlusDispel:
                case (int)SkillNames.SuperGM.Resurrection:
                    {
                        byte targets = packet.ReadByte();
                        Func<Character, bool> condition = null;
                        Action<Character> action = null;

                        switch (skill.MapleId)
                        {
                            case (int)SkillNames.SuperGM.HealPlusDispel:
                                {
                                    condition = new Func<Character, bool>((target) => target.IsAlive);
                                    action = new Action<Character>((target) =>
                                    {
                                        target.Health = target.MaxHealth;
                                        target.Mana = target.MaxMana;

                                        // TODO: Use dispell.
                                    });
                                }
                                break;

                            case (int)SkillNames.SuperGM.Resurrection:
                                {
                                    condition = new Func<Character, bool>((target) => !target.IsAlive);
                                    action = new Action<Character>((target) =>
                                    {
                                        target.Health = target.MaxHealth;
                                    });
                                }
                                break;

                            default:
                                {
                                    condition = new Func<Character, bool>((target) => true);
                                    action = new Action<Character>((target) =>
                                    {
                                        target.Buffs.Add(skill, 0);
                                    });
                                }
                                break;
                        }

                        for (byte i = 0; i < targets; i++)
                        {
                            int targetId = packet.ReadInt();

                            Character target = skill.Character.Map.Characters[targetId];

                            if (target != skill.Character && condition(target))
                            {
                                using (var oPacket = new PacketWriter(ServerOperationCode.Effect))
                                {
                                    oPacket.WriteByte((byte)UserEffect.SkillAffected);
                                    oPacket.WriteInt(skill.MapleId);
                                    oPacket.WriteByte(1);
                                    oPacket.WriteByte(1);

                                    target.Client.Send(oPacket);
                                }

                                using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
                                {

                                    oPacket.WriteInt(target.Id);
                                    oPacket.WriteByte((byte)UserEffect.SkillAffected);
                                    oPacket.WriteInt(skill.MapleId);
                                    oPacket.WriteByte(1);
                                    oPacket.WriteByte(1);

                                    target.Map.Broadcast(oPacket, target);
                                }

                                action(target);
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

            using (var oPacket = new PacketWriter(ServerOperationCode.RemoteEffect))
            {

                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteByte((byte)UserEffect.SkillUse);
                oPacket.WriteInt(skill.MapleId);
                oPacket.WriteByte(1);
                oPacket.WriteByte(1);

                skill.Character.Map.Broadcast(oPacket, skill.Character);
            }

            if (skill.HasBuff)
            {
                skill.Character.Buffs.Add(skill, 0);
            }
        }
    }
}