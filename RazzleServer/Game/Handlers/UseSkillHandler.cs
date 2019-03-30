using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Packets;
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
                client.Character.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            if (!client.Character.IsAlive)
            {
                client.Character.LogCheatWarning(CheatType.InvalidSkillChange);
                return;
            }

            //MapPacket.SendPlayerSkillAnim(chr, SkillID, SkillLevel);

            client.Character.Buffs.Add(skill, 0);
            
            switch (skillId)
            {
                case (int)SkillNames.Spearman.HyperBody:
                    {
                        ushort healRate = (ushort)skill.Hp;
                        if (healRate > 100)
                        {
                            healRate = 100;
                        }

                        short healAmount = (short)(healRate * chr.PrimaryStats.GetMaxHP(false) / 100); // Party: / (amount players)
                        if (!chr.Buffs.mActiveSkillLevels.ContainsKey(SkillID))
                        {
                            chr.ModifyMaxHP(healAmount);
                        }
                        break;
                    }
                case (int)SkillNames.Cleric.Heal:
                    {
                        ushort healRate = (ushort)sld.HPProperty;
                        if (healRate > 100)
                        {
                            healRate = 100;
                        }

                        short healAmount = (short)(healRate * chr.PrimaryStats.GetMaxHP(false) / 100); // Party: / (amount players)

                        chr.ModifyHP(healAmount, true);
                        break;
                    }
                case (int)SkillNames.Gm.Hide:
                    {
                        client.Character.Map.Characters.Remove(client.Character);
                        DataProvider.Maps[chr.Map].RemovePlayer(chr, true);
                        AdminPacket.Hide(client, true);
                        break;
                    }
                case (int)SkillNames.Priest.MysticDoor:
                    {
                        Door door = new Door(chr, chr.Map, DataProvider.Maps[chr.Map].ReturnMap, chr.Position.X, chr.Position.Y);
                        chr.Door = door;
                        MapPacket.SpawnDoor(chr, true, chr.Position.X, chr.Position.Y);
                        MapPacket.SpawnPortal(chr, chr.Position);
                        client.Character.Release();
                        break;
                    }
                case (int)SkillNames.Gm.Haste:
                case (int)SkillNames.Gm.HolySymbol:
                case (int)SkillNames.Gm.Bless:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null && victim != chr)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.Buffs.AddBuff(SkillID, SkillLevel);
                            }
                        }
                        break;
                    }
                case (int)SkillNames.Gm.HealPlusDispell:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.ModifyHP(victim.PrimaryStats.GetMaxMP(false), true);
                                victim.ModifyMP(victim.PrimaryStats.GetMaxMP(false), true);
                            }
                        }
                        chr.ModifyHP(chr.PrimaryStats.GetMaxMP(false), true);
                        chr.ModifyMP(chr.PrimaryStats.GetMaxMP(false), true);
                        break;
                    }
                case (int)SkillNames.Gm.Resurrection:
                    {
                        byte players = packet.ReadByte();
                        for (byte i = 0; i < players; i++)
                        {
                            int playerid = packet.ReadInt();
                            Character victim = DataProvider.Maps[chr.Map].GetPlayer(playerid);
                            if (victim != null && victim.PrimaryStats.HP <= 0)
                            {
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, true);
                                MapPacket.SendPlayerSkillAnimThirdParty(victim, SkillID, SkillLevel, true, false);
                                victim.ModifyHP(victim.PrimaryStats.GetMaxHP(false), true);
                            }
                        }
                        break;
                    }
            }
            
            client.Character.Release();
            chr.Skills.DoSkillCost(SkillID, SkillLevel);
            if (Constants.isSummon(SkillID))
            {
                chr.Summons.NewSummon(SkillID, SkillLevel);
            }  
        }
    }
}
