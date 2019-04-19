using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.SkillUse)]
    public class SkillUseHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var skillId = packet.ReadInt();
            var skillLevel = packet.ReadByte();
            var skill = client.GameCharacter.Skills[skillId];

            if (skill.CurrentLevel != skillLevel)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            if (!client.GameCharacter.IsAlive)
            {
                client.GameCharacter.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            client.GameCharacter.Buffs.AddBuff(skill.MapleId, skill.CurrentLevel);

            switch (skillId)
            {
                case (int)SkillNames.Spearman.HyperBody:
                {
                    var healRate = (ushort)skill.Hp;
                    if (healRate > 100)
                    {
                        healRate = 100;
                    }

                    var healAmount =
                        (short)(healRate * client.GameCharacter.PrimaryStats.MaxHealth /
                                100); // Party: / (amount players)
                    if (!client.GameCharacter.PrimaryStats.HasBuff(skillId))
                    {
                        client.GameCharacter.PrimaryStats.MaxHealth += healAmount;
                    }

                    break;
                }

                case (int)SkillNames.Cleric.Heal:
                {
                    var healRate = (ushort)skill.Hp;
                    if (healRate > 100)
                    {
                        healRate = 100;
                    }

                    var healAmount =
                        (short)(healRate * client.GameCharacter.PrimaryStats.MaxHealth /
                                100); // Party: / (amount players)

                    client.GameCharacter.PrimaryStats.Health += healAmount;
                    break;
                }

                case (int)SkillNames.Gm.Hide:
                {
                    client.GameCharacter.Hide(true);
                    break;
                }

                case (int)SkillNames.Priest.MysticDoor:
                {
                    //var door = new Door(chr, client.Character.Map, DataProvider.Maps[client.Character.Map].ReturnMap, client.Character.Position.X, client.Character.Position.Y);
                    //client.Character.Door = door;
                    //client.Character.Map.Doors.Add(door);
                    //MapPacket.SpawnDoor(chr, true, client.Character.Position.X, client.Character.Position.Y);
                    //MapPacket.SpawnPortal(chr, client.Character.Position);
                    client.GameCharacter.Release();
                    break;
                }

                case (int)SkillNames.Gm.Haste:
                case (int)SkillNames.Gm.HolySymbol:
                case (int)SkillNames.Gm.Bless:
                {
                    var players = packet.ReadByte();
                    for (byte i = 0; i < players; i++)
                    {
                        var playerid = packet.ReadInt();
                        var victim = client.Server.GetCharacterById(playerid);
                        if (victim != null && victim.Id != client.GameCharacter.Id)
                        {
                            //victim.Buffs.AddBuff(SkillID, SkillLevel);
                        }
                    }

                    break;
                }

                case (int)SkillNames.Gm.HealPlusDispell:
                {
                    var players = packet.ReadByte();
                    for (byte i = 0; i < players; i++)
                    {
                        var playerid = packet.ReadInt();
                        var victim = client.Server.GetCharacterById(playerid);
                        if (victim != null)
                        {
                            //MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                            //MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                            victim.PrimaryStats.Health = victim.PrimaryStats.MaxHealth;
                            victim.PrimaryStats.Mana = victim.PrimaryStats.MaxMana;
                        }
                    }

                    client.GameCharacter.PrimaryStats.Health = client.GameCharacter.PrimaryStats.MaxHealth;
                    client.GameCharacter.PrimaryStats.Mana = client.GameCharacter.PrimaryStats.MaxMana;
                    break;
                }

                case (int)SkillNames.Gm.Resurrection:
                {
                    var players = packet.ReadByte();
                    for (byte i = 0; i < players; i++)
                    {
                        var playerid = packet.ReadInt();
                        var victim = client.Server.GetCharacterById(playerid);
                        if (victim != null && !victim.IsAlive)
                        {
                            //MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                            //MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                            victim.PrimaryStats.Health = victim.PrimaryStats.MaxHealth;
                        }
                    }

                    break;
                }

                case (int)SkillNames.Priest.SummonDragon:
                case (int)SkillNames.Ranger.SilverHawk:
                case (int)SkillNames.Sniper.GoldenEagle:
                case (int)SkillNames.Ranger.Puppet:
                case (int)SkillNames.Sniper.Puppet:
                {
                    var position = packet.ReadPoint();
                    client.GameCharacter.Summons.Add(skill, position);
                    break;
                }
            }

            client.GameCharacter.Release();
            skill.Cast();
        }
    }
}
