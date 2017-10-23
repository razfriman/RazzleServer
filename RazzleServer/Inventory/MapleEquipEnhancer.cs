using Microsoft.Extensions.Logging;
using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Inventory
{
    public static class MapleEquipEnhancer
    {
        private static ILogger Log = LogManager.Log;

        public static void UseEquipEnhancementScroll(MapleEquip equip, MapleItem scroll, MapleCharacter chr)
        {
            if (equip == null || scroll == null) return;
            WzItemEnhancer wzScrollInfo = DataBuffer.GetItemById(scroll.ItemId) as WzItemEnhancer;
            WzEquip equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            if (wzScrollInfo == null || equipInfo == null) return;
            int maxEnhancements;
            if (wzScrollInfo == null || equipInfo == null || (maxEnhancements = equipInfo.MaxStarEnhance) - equip.Enhancements < 1 || equip.RemainingUpgradeCount > 0)
            {
                chr.SendPopUpMessage("You cannot use that on this item.");
                chr.EnableActions();
                return;
            }
            int chance;
            bool diminishChance = false;
            int enhancements;
            int curseChance = 100;
            switch (scroll.ItemId)
            {
                case 2049323:   //advanced equip enhancement scroll
                    curseChance = 0;
                    goto case 2049300;
                case 2049300:
                case 2049303:
                case 2049306:
                case 2049325:
                    {
                        chance = 100;
                        diminishChance = true;
                        enhancements = 1;
                        break;
                    }
                case 2049301: //equip enhancement scroll
                case 2049307:
                    {
                        chance = 80;
                        diminishChance = true;
                        enhancements = 1;
                        break;
                    }
                default:
                    {
                        if (wzScrollInfo.StatEnhancements.TryGetValue("forceUpgrade", out enhancements) && //multiple star enhances
                            wzScrollInfo.StatEnhancements.TryGetValue("success", out chance))
                        {
                            if (!wzScrollInfo.StatEnhancements.TryGetValue("cursed", out curseChance))
                                curseChance = 100;
                            break;
                        }

                        chr.SendPopUpMessage("This item is not coded, please report it on the forums. ItemId " + scroll.ItemId);
                        Log.LogDebug($"ItemID {scroll.ItemId} is unhandled in UseEnhancementScroll");
                        return;
                    }
            }
            bool scrollProtection = equip.CheckAndRemoveFlag(MapleItemFlags.ScrollProtection);
            bool curseProtection = equip.CheckAndRemoveFlag(MapleItemFlags.CurseProtection) && equip.Enhancements < 12;
            bool success = EnhanceEquip(equip, chance, diminishChance, enhancements, maxEnhancements);
            bool destroyed = false;
            byte scrollResult = 1;
            bool removeScroll = success || !scrollProtection;
            if (!success)
            {
                scrollResult = 0;
                if (!curseProtection && Functions.MakeChance(curseChance))
                {
                    scrollResult = 2;
                    destroyed = true;
                }
            }
            if (removeScroll)
                chr.Inventory.RemoveItemsFromSlot(scroll.InventoryType, scroll.Position, 1);
            if (destroyed)
                chr.Inventory.RemoveItem(equip.InventoryType, equip.Position);
            else
                chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, equip.InventoryType, equip.Position)); //Update item
            chr.Map.BroadcastPacket(Packets.ShowScrollEffect(chr.ID, scrollResult, scroll.ItemId, equip.ItemId), chr, true);
        }

        private static bool EnhanceEquip(MapleEquip equip, int chance, bool diminishChance, int enhancements, int maxEnhancements)
        {
            if (diminishChance)
                chance = chance - (equip.Enhancements * 10); //10% less chance for every enhancement level
            chance = Math.Max(chance, 5); //Always minimum 5% chance
            if (!Functions.MakeChance(chance))
                return false;
            for (int i = 0; i < enhancements && equip.Enhancements < maxEnhancements; i++)
            {
                equip.Enhancements++;
                if (equip.Str > 0 || Functions.MakeChance(2)) equip.Str += (short)Functions.Random(0, 4);
                if (equip.Dex > 0 || Functions.MakeChance(2)) equip.Dex += (short)Functions.Random(0, 4);
                if (equip.Int > 0 || Functions.MakeChance(2)) equip.Int += (short)Functions.Random(0, 4);
                if (equip.Luk > 0 || Functions.MakeChance(2)) equip.Luk += (short)Functions.Random(0, 4);

                if (equip.Pad > 0 && equip.IsWeapon) equip.Pad += (short)Functions.Random(0, 4);
                if (equip.Mad > 0 && equip.IsWeapon) equip.Mad += (short)Functions.Random(0, 4);

                if (equip.Pdd > 0 || Functions.MakeChance(3)) equip.Pdd += (short)Functions.Random(0, 4);
                if (equip.Mdd > 0 || Functions.MakeChance(3)) equip.Mdd += (short)Functions.Random(0, 4);

                if (equip.Acc > 0 || Functions.MakeChance(5)) equip.Acc += (short)Functions.Random(0, 4);
                if (equip.Eva > 0 || Functions.MakeChance(5)) equip.Eva += (short)Functions.Random(0, 4);
                if (equip.Speed > 0 || Functions.MakeChance(5)) equip.Speed += (short)Functions.Random(0, 4);
                if (equip.Jump > 0 || Functions.MakeChance(5)) equip.Jump += (short)Functions.Random(0, 4);

                if (equip.IncMhp > 0 || Functions.MakeChance(20)) equip.IncMhp += (short)Functions.Random(0, 4);
                if (equip.IncMmp > 0 || Functions.MakeChance(20)) equip.IncMmp += (short)Functions.Random(0, 4);
            }
            return true;
        }

        public static void UseSpecialEquipScroll(MapleEquip equip, MapleItem scroll, MapleCharacter chr)
        {

            WzEquip equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            if (equipInfo == null || equipInfo.IsCashItem) return;
            bool success = false;
            switch (scroll.ItemId)
            {
                //Safety scroll:
                case 5064101: //Safety Scroll Lite
                    {
                        if (equipInfo.ReqLevel > 105)
                            break;
                        goto case 2532000;
                    }
                case 2532001: // Pet equip Safety Scrolls
                case 2532004:
                case 5068100:
                    {
                        //todo: check if it's a pet equip
                        goto case 2532000;
                    }
                case 2532000:
                case 2532002:
                case 2532003:
                case 5064100:
                    {
                        if (equip.RemainingUpgradeCount > 0 && !equip.Flags.HasFlag(MapleItemFlags.UpgradeCountProtection))
                        {
                            equip.Flags |= MapleItemFlags.UpgradeCountProtection;
                            success = true;
                        }
                        break;
                    }
                //Recovery scroll:
                case 5064301: //lite
                    {
                        if (equipInfo.ReqLevel > 105)
                            break;
                        goto case 2533000;
                    }
                case 2533001:
                case 2533002:
                case 5068200:
                    {
                        //todo: check if it's a pet equip
                        goto case 2533000;
                    }
                case 2533000:
                case 5064300:
                    {
                        if (!equip.Flags.HasFlag(MapleItemFlags.ScrollProtection))
                        {
                            equip.Flags |= MapleItemFlags.ScrollProtection;
                            success = true;
                        }
                        break;
                    }
                //Lucky day scroll:
                case 5068000:
                case 2530003:
                case 2530006:
                    {
                        //todo: check if it's a pet equip
                        goto case 2530000;
                    }
                case 2530000:
                case 2530002:
                case 2530004:
                case 5063000:
                    {
                        if (equip.RemainingUpgradeCount > 0 && !equip.Flags.HasFlag(MapleItemFlags.LuckyDay))
                        {
                            equip.Flags |= MapleItemFlags.LuckyDay;
                            success = true;
                        }
                        break;
                    }
                //Protection scroll:
                case 5064002: //Protection Scroll Lite
                    {
                        if (equipInfo.ReqLevel > 105)
                            break;
                        goto case 2531000;
                    }
                case 2531000:
                case 2531001:
                case 2531004:
                case 2531005:
                case 2531007:
                case 5064000:
                    {
                        if (equip.Enhancements < 12 && !equip.Flags.HasFlag(MapleItemFlags.CurseProtection))
                        {
                            equip.Flags |= MapleItemFlags.CurseProtection;
                            success = true;
                        }
                        break;
                    }
            }
            if (success)
            {
                chr.Inventory.RemoveItemsFromSlot(scroll.InventoryType, scroll.Position, 1);
                chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, MapleInventoryType.Equip, equip.Position));
                chr.Client.SendPacket(Packets.ShowScrollEffect(chr.ID, 1, scroll.ItemId, equip.ItemId));
            }
            else
            {
                chr.SendPopUpMessage("You cannot use that on this item");
            }
        }

        public static void UseRegularEquipScroll(MapleCharacter chr, MapleEquip equip, MapleItem scroll, bool whiteScroll)
        {
            WzItemEnhancer scrollInfo = DataBuffer.GetItemById(scroll.ItemId) as WzItemEnhancer;
            if (scrollInfo == null) return;
            WzEquip equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            string reason = "";
            if (equipInfo == null || !CanScroll(equip, equipInfo, scrollInfo, out reason))
            {
                chr.SendPopUpMessage("You cannot use that scroll on this item. " + reason);
                chr.EnableActions();
                return;
            }
            byte upgradeSlotsUsed = 1;
            int scrollBaseId = scroll.ItemId / 100;
            if (scrollBaseId == 20490 ||    //clean slate scroll
                scrollBaseId == 20493 ||    //equip enhancement scroll
                scrollBaseId == 20496)      //innocence scroll
                upgradeSlotsUsed = 0;

            switch (scroll.ItemId)
            {
                case 2040727: //Scroll for Spikes on Shoes
                case 2041058: //Scroll for Cape for Cold Protection
                    upgradeSlotsUsed = 0;
                    break;
            }
            int value;
            if (scrollInfo.StatEnhancements.TryGetValue("tuc", out value)) //Uses multiple upgrade slots
                upgradeSlotsUsed = (byte)value;
            if (equip.RemainingUpgradeCount < upgradeSlotsUsed)
            {
                chr.SendPopUpMessage(string.Format("This item does not have enough upgrades available"));
                chr.EnableActions();
                return;
            }
            byte scrollResult = 0;

            bool destroyed = false;
            bool removeScroll = true;
            int chance = scrollInfo.Chance;
            bool scrollProtection = equip.CheckAndRemoveFlag(MapleItemFlags.ScrollProtection);
            bool upgradeCountProtection = upgradeSlotsUsed > 0 && equip.CheckAndRemoveFlag(MapleItemFlags.UpgradeCountProtection);
            bool curseProtection = equip.CheckAndRemoveFlag(MapleItemFlags.CurseProtection);
            if (equip.CheckAndRemoveFlag(MapleItemFlags.LuckyDay))
                chance += 10;

            if (Functions.MakeChance(chance))
            {
                scrollResult = 1;
                equip.UpgradeCount += upgradeSlotsUsed;
                foreach (var kvp in scrollInfo.StatEnhancements)
                {
                    switch (kvp.Key)
                    {
                        case "incPDD":
                            equip.Pdd += (short)kvp.Value;
                            break;
                        case "incMDD":
                            equip.Mdd += (short)kvp.Value;
                            break;
                        case "incACC":
                            equip.Acc += (short)kvp.Value;
                            break;
                        case "incMHP":
                            equip.IncMhp += (short)kvp.Value;
                            break;
                        case "incINT":
                            equip.Int += (short)kvp.Value;
                            break;
                        case "incDEX":
                            equip.Dex += (short)kvp.Value;
                            break;
                        case "incMAD":
                            equip.Mad += (short)kvp.Value;
                            break;
                        case "incPAD":
                            equip.Pad += (short)kvp.Value;
                            break;
                        case "incEVA":
                            equip.Eva += (short)kvp.Value;
                            break;
                        case "incLUK":
                            equip.Luk += (short)kvp.Value;
                            break;
                        case "incMMP":
                            equip.IncMmp += (short)kvp.Value;
                            break;
                        case "incSTR":
                            equip.Str += (short)kvp.Value;
                            break;
                        case "incSpeed":
                            equip.Speed += (short)kvp.Value;
                            break;
                        case "incJump":
                            equip.Jump += (short)kvp.Value;
                            break;
                        case "incCraft":
                            equip.Diligence += (short)kvp.Value;
                            break;
                        case "preventslip":
                            equip.Flags |= MapleItemFlags.NoSlip;
                            break;
                        case "warmsupport":
                            equip.Flags |= MapleItemFlags.ColdResistance;
                            break;
                        case "recover":
                            {
                                int totalPossibleUpgrades = equipInfo.TotalUpgradeCount + equip.HammersApplied;
                                int failedUpgrades = totalPossibleUpgrades - equip.UpgradeCount - equip.RemainingUpgradeCount;
                                int toRestore = Math.Min(failedUpgrades, kvp.Value);
                                if (toRestore > 0)
                                    equip.RemainingUpgradeCount += (byte)toRestore;
                                break;
                            }
                        case "randstat": //chaos scroll
                            {
                                bool betterStats = scrollInfo.StatEnhancements.ContainsKey("incRandVol");
                                bool noNegative = scrollInfo.StatEnhancements.ContainsKey("noNegative");

                                if (equip.Str > 0) equip.Str = GetNewChaosStat(equip.Str, betterStats, noNegative);
                                if (equip.Dex > 0) equip.Dex = GetNewChaosStat(equip.Dex, betterStats, noNegative);
                                if (equip.Int > 0) equip.Int = GetNewChaosStat(equip.Int, betterStats, noNegative);
                                if (equip.Luk > 0) equip.Luk = GetNewChaosStat(equip.Luk, betterStats, noNegative);
                                if (equip.Pad > 0) equip.Pad = GetNewChaosStat(equip.Pad, betterStats, noNegative);
                                if (equip.Mad > 0) equip.Mad = GetNewChaosStat(equip.Mad, betterStats, noNegative);
                                if (equip.Pdd > 0) equip.Pdd = GetNewChaosStat(equip.Pdd, betterStats, noNegative);
                                if (equip.Mdd > 0) equip.Mdd = GetNewChaosStat(equip.Mdd, betterStats, noNegative);
                                if (equip.Acc > 0) equip.Acc = GetNewChaosStat(equip.Acc, betterStats, noNegative);
                                if (equip.Eva > 0) equip.Eva = GetNewChaosStat(equip.Eva, betterStats, noNegative);
                                if (equip.Speed > 0) equip.Speed = GetNewChaosStat(equip.Speed, betterStats, noNegative);
                                if (equip.Jump > 0) equip.Jump = GetNewChaosStat(equip.Jump, betterStats, noNegative);
                                if (equip.IncMhp > 0) equip.IncMhp = GetNewChaosStat(equip.IncMhp, betterStats, noNegative);
                                if (equip.IncMmp > 0) equip.IncMmp = GetNewChaosStat(equip.IncMmp, betterStats, noNegative);
                                break;
                            }
                        case "reset":
                            {
                                equip.SetDefaultStats(equipInfo, true);
                                break;
                            }
                        case "perfectReset":
                            {
                                equip.SetDefaultStats(equipInfo);
                                break;
                            }

                    }
                    equip.SaveToDatabase(chr);
                }
            }
            else //Fail
            {
                int cursedChance;
                if (scrollInfo.StatEnhancements.TryGetValue("cursed", out cursedChance))
                {
                    destroyed = !curseProtection && Functions.MakeChance(cursedChance);
                }
                if (scrollProtection)
                    removeScroll = false;
                if (upgradeCountProtection)
                    upgradeSlotsUsed = 0;
                //todo: white scroll
            }

            equip.RemainingUpgradeCount -= upgradeSlotsUsed;

            if (removeScroll)
                chr.Inventory.RemoveItemsFromSlot(scroll.InventoryType, scroll.Position, 1);

            if (destroyed)
            {
                chr.Inventory.RemoveItem(equip.InventoryType, equip.Position);
                scrollResult = 2;
            }
            else
                chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, equip.InventoryType, equip.Position)); //Update item stats
            chr.Map.BroadcastPacket(Packets.ShowScrollEffect(chr.ID, scrollResult, scroll.ItemId, equip.ItemId), chr, true);
        }

        public static bool UsePotentialScroll(MapleEquip equip, MapleItem scroll, MapleCharacter chr)
        {
            int successChance;
            int curseChance = 0;
            MaplePotentialState grade = MaplePotentialState.Rare;
            switch (scroll.ItemId)
            {
                //Potential scrolls:
                case 2049401:
                case 2049408:
                    {
                        successChance = 70;
                        curseChance = 100;
                        break;
                    }
                case 2049416:
                    {
                        successChance = 70;
                        break;
                    }
                case 2049400:
                case 2049407:
                case 2049412:
                case 2049413:
                    {
                        successChance = 90;
                        curseChance = 100;
                        break;
                    }
                case 2049402:
                case 2049417:
                case 2049406:
                    {
                        successChance = 100;
                        break;
                    }
                //Epic potential scrolls:
                case 2049700:
                //case 2049702: ?
                case 2049703:
                case 2049706:
                    {
                        successChance = 100;
                        grade = MaplePotentialState.Epic;
                        break;
                    }
                case 2049702:
                    {
                        successChance = 80;
                        curseChance = 20;
                        grade = MaplePotentialState.Epic;
                        break;
                    }
                case 2049704:
                case 2049707:
                    {
                        successChance = 40;
                        grade = MaplePotentialState.Epic;
                        break;
                    }
                case 2049709:
                    {
                        curseChance = 50;
                        goto case 2049705;
                    }
                case 2049705:
                case 2049708:
                case 2049710:
                    {
                        successChance = 50;
                        grade = MaplePotentialState.Epic;
                        break;
                    }
                //Unique potential scrolls:
                case 2049750:
                case 2049753:
                    {
                        successChance = 80;
                        grade = MaplePotentialState.Unique;
                        break;
                    }
                case 2049751:
                case 2049754:
                    {
                        successChance = 60;
                        grade = MaplePotentialState.Unique;
                        break;
                    }
                case 2049757:
                case 2049758:
                    {
                        successChance = 50;
                        grade = MaplePotentialState.Unique;
                        break;
                    }
                case 2049752:
                case 2049755:
                case 2049756:
                    {
                        successChance = 30;
                        grade = MaplePotentialState.Unique;
                        break;
                    }
                default:
                    {
                        chr.SendPopUpMessage("You cannot use this item");
                        chr.EnableActions();
                        return false;
                    }
            }
            //Item has potential but it's hidden or is same rank as the potential given by the scroll
            if (equip.PotentialState != MaplePotentialState.None && (equip.PotentialState < MaplePotentialState.Rare || equip.PotentialState >= grade))
                return false;
            if (equip.CheckAndRemoveFlag(MapleItemFlags.LuckyDay))
                successChance += 10;
            bool destroyed = false;
            bool removeScroll = true;
            bool scrollProtection = equip.CheckAndRemoveFlag(MapleItemFlags.ScrollProtection);
            bool curseProtection = curseChance > 0 && equip.CheckAndRemoveFlag(MapleItemFlags.CurseProtection);
            byte scrollResult;
            if (Functions.MakeChance(successChance))
            {
                scrollResult = 1;
                AddRandomPotential(equip);
            }
            else
            {
                scrollResult = 0;
                destroyed = !curseProtection && Functions.MakeChance(curseChance);
                removeScroll = !scrollProtection;
            }

            if (removeScroll)
                chr.Inventory.RemoveItemsFromSlot(scroll.InventoryType, scroll.Position, 1);

            if (destroyed)
            {
                chr.Inventory.RemoveItem(equip.InventoryType, equip.Position);
                scrollResult = 2;
            }
            else
                chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, equip.InventoryType, equip.Position)); //Update item stats
            chr.Map.BroadcastPacket(Packets.ShowScrollEffect(chr.ID, scrollResult, scroll.ItemId, equip.ItemId), chr, true);
            return true;
        }

        public static bool UseBonusPotentialScroll(MapleEquip equip, MapleItem scroll, MapleCharacter chr)
        {
            if (equip.PotentialState == MaplePotentialState.None) return false; //Can only be used on items that already have regular potential
            int successChance;
            int curseChance;
            switch (scroll.ItemId)
            {
                case 2048305:
                    {
                        successChance = 70;
                        curseChance = 100;
                        break;
                    }
                case 2048308:
                case 2048311:
                    {
                        successChance = 50;
                        curseChance = 50;
                        break;
                    }
                case 2048309:
                case 2048314:
                    {
                        successChance = 60;
                        curseChance = 0;
                        break;
                    }
                case 2048310:
                    {
                        successChance = 60;
                        curseChance = 100;
                        break;
                    }
                case 2048307:
                case 2048315:
                case 2048329:
                    {
                        successChance = 100;
                        curseChance = 0;
                        break;
                    }
                default:
                    {
                        chr.SendPopUpMessage("You cannot use this item.");
                        chr.EnableActions();
                        return false;
                    }
            }
            if (equip.CheckAndRemoveFlag(MapleItemFlags.LuckyDay))
                successChance += 10;
            bool destroyed = false;
            bool removeScroll = true;
            bool scrollProtection = equip.CheckAndRemoveFlag(MapleItemFlags.ScrollProtection);
            bool curseProtection = curseChance > 0 && equip.CheckAndRemoveFlag(MapleItemFlags.CurseProtection);
            byte scrollResult;
            if (Functions.MakeChance(successChance))
            {
                scrollResult = 1;
                AddRandomBonusPotential(equip);
            }
            else
            {
                scrollResult = 0;
                destroyed = !curseProtection && Functions.MakeChance(curseChance);
                removeScroll = !scrollProtection;
            }

            if (removeScroll)
                chr.Inventory.RemoveItemsFromSlot(scroll.InventoryType, scroll.Position, 1);

            if (destroyed)
            {
                chr.Inventory.RemoveItem(equip.InventoryType, equip.Position);
                scrollResult = 2;
            }
            else
                chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, equip.InventoryType, equip.Position)); //Update item stats
            chr.Map.BroadcastPacket(Packets.ShowScrollEffect(chr.ID, scrollResult, scroll.ItemId, equip.ItemId), chr, true);
            return true;
        }

        private static bool CanAddPotentialOnEquip(MapleEquip equip, MaplePotentialState minimumPotential = MaplePotentialState.None)
        {
            if (equip.PotentialState >= MaplePotentialState.HiddenRare)
            {
                if (equip.PotentialState <= MaplePotentialState.HiddenLegendary) return false; //not sure if you can use it on hidden
                if (equip.PotentialState > minimumPotential) return false;
            }
            else
            {
                //Check if this
            }
            return true;
        }

        private static bool CanAddBonusPotentialOnEquip(MapleEquip equip)
        {
            return equip.PotentialState > MaplePotentialState.None;
        }

        public static void AddRandomPotential(MapleEquip equip, MaplePotentialState minimumGrade = MaplePotentialState.HiddenRare, WzEquip equipInfo = null)
        {
            if (equipInfo == null)
                equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            // 0.25% chance for unique, 5% chance for epic, otherwise rare
            if (minimumGrade > MaplePotentialState.HiddenLegendary)
                minimumGrade -= 16;
            equip.PotentialState = Functions.MakeChance(5) ? (Functions.MakeChance(5) ? MaplePotentialState.HiddenUnique : MaplePotentialState.HiddenEpic) : MaplePotentialState.HiddenRare;
            if (equip.PotentialState < minimumGrade) equip.PotentialState = minimumGrade;
            MaplePotentialState lowerGrade = equip.PotentialState == MaplePotentialState.HiddenRare ? MaplePotentialState.HiddenRare : equip.PotentialState - 1;
            equip.Potential1 = DataBuffer.GetRandomPotential(equip.PotentialState, equipInfo.ReqLevel, equip.ItemId);
            equip.Potential2 = DataBuffer.GetRandomPotential(Functions.MakeChance(20) ? equip.PotentialState : lowerGrade, equipInfo.ReqLevel, equip.ItemId); // 20% chance to be of the same grade as line 1, otherwise 1 lower 
            equip.Potential3 = Functions.MakeChance(7) ? DataBuffer.GetRandomPotential(lowerGrade, equipInfo.ReqLevel, equip.ItemId) : (ushort)0; // 7% chance for 3rd line
        }

        public static void AddRandomBonusPotential(MapleEquip equip, MaplePotentialState grade = MaplePotentialState.Rare, WzEquip equipInfo = null)
        {
            if (equipInfo == null)
                equipInfo = DataBuffer.GetEquipById(equip.ItemId);
        }

        public static bool CubeItem(MapleEquip equip, CubeType cubeType, MapleCharacter chr, bool reveal = false)
        {
            WzEquip equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            if (equipInfo == null || !CanCube(equip, equipInfo)) return false;
            int currentLines = equip.Potential3 > 0 ? 3 : 2;
            double lineIncreaseChance = 0;
            double gradeIncreaseChance = 0;
            double gradeDecreaseChance = 0;
            int otherLinesIncreaseChance = 0; //chance to increase the other lines' grade to the current grade
            switch (cubeType)
            {
                case CubeType.Occult:
                    {
                        switch (equip.PotentialState)
                        {
                            case MaplePotentialState.Rare:
                                {
                                    gradeIncreaseChance = 4;
                                    break;
                                }
                            case MaplePotentialState.Epic:
                                {
                                    gradeIncreaseChance = 2;
                                    gradeDecreaseChance = 2;
                                    otherLinesIncreaseChance = 15;
                                    break;
                                }
                            case MaplePotentialState.Unique:
                                {
                                    gradeDecreaseChance = 3;
                                    otherLinesIncreaseChance = 5;
                                    break;
                                }
                            default:
                                return false;
                        }
                        break;
                    }
                case CubeType.MasterCraftsman:
                    {
                        switch (equip.PotentialState)
                        {
                            case MaplePotentialState.Rare:
                                {
                                    gradeIncreaseChance = 5;
                                    break;
                                }
                            case MaplePotentialState.Epic:
                                {
                                    otherLinesIncreaseChance = 20;
                                    gradeIncreaseChance = 3;
                                    break;
                                }
                            case MaplePotentialState.Unique:
                                otherLinesIncreaseChance = 5;
                                break;
                            default:
                                return false;
                        }
                        break;
                    }
                case CubeType.Meister:
                    {
                        lineIncreaseChance = 5;
                        switch (equip.PotentialState)
                        {
                            case MaplePotentialState.Rare:
                                {
                                    gradeIncreaseChance = 6;
                                    break;
                                }
                            case MaplePotentialState.Epic:
                                {
                                    otherLinesIncreaseChance = 30;
                                    gradeIncreaseChance = 4;
                                    break;
                                }
                            case MaplePotentialState.Unique:
                                {
                                    otherLinesIncreaseChance = 20;
                                    gradeIncreaseChance = 2;
                                    break;
                                }
                            case MaplePotentialState.Legendary:
                                {
                                    otherLinesIncreaseChance = 10;
                                    break;
                                }
                            default:
                                return false;
                        }
                        break;
                    }
                case CubeType.PlatinumMiracle:
                    {
                        lineIncreaseChance = 7;
                        switch (equip.PotentialState)
                        {
                            case MaplePotentialState.Rare:
                                {
                                    gradeIncreaseChance = 15;
                                    break;
                                }
                            case MaplePotentialState.Epic:
                                {
                                    otherLinesIncreaseChance = 40;
                                    gradeIncreaseChance = 10;
                                    break;
                                }
                            case MaplePotentialState.Unique:
                                {
                                    otherLinesIncreaseChance = 30;
                                    gradeIncreaseChance = 4;
                                    break;
                                }
                            case MaplePotentialState.Legendary:
                                {
                                    otherLinesIncreaseChance = 20;
                                    break;
                                }
                            default:
                                return false;
                        }
                        break;
                    }
                default:
                    return false;
            }
            if (currentLines == 2 && Functions.MakeChance(lineIncreaseChance))
                currentLines = 3;
            if (Functions.MakeChance(gradeIncreaseChance) && equip.PotentialState < MaplePotentialState.Legendary)
                equip.PotentialState++;
            else if (Functions.MakeChance(gradeDecreaseChance) && equip.PotentialState > MaplePotentialState.Rare)
            {
                equip.PotentialState--;
            }
            ushort[] newLines = new ushort[currentLines];
            newLines[0] = DataBuffer.GetRandomPotential(equip.PotentialState, equipInfo.ReqLevel, equip.ItemId);
            for (int i = 1; i < newLines.Length; i++)
            {
                if (equip.PotentialState == MaplePotentialState.Rare || Functions.MakeChance(otherLinesIncreaseChance))
                    newLines[i] = DataBuffer.GetRandomPotential(equip.PotentialState, equipInfo.ReqLevel, equip.ItemId);
                else
                    newLines[i] = DataBuffer.GetRandomPotential(equip.PotentialState - 1, equipInfo.ReqLevel, equip.ItemId);
            }
            newLines = newLines.OrderByDescending(x => x).ToArray();

            equip.Potential1 = newLines[0];
            equip.Potential2 = newLines[1];
            if (currentLines == 3)
                equip.Potential3 = newLines[2];
            if (!reveal)
                equip.PotentialState -= 16; // Hide potential, needs to be revealed    
            equip.SaveToDatabase(chr);
            chr.Client.SendPacket(MapleInventory.Packets.AddItem(equip, MapleInventoryType.Equip, equip.Position));
            return true;
        }

        private static bool CanCube(MapleEquip equip, WzEquip equipInfo)
        {
            if (equip.PotentialState < MaplePotentialState.Rare) return false; //Potential is hidden or non-existant, can't be cubed
            return true;
        }

        private static bool CanScroll(MapleEquip equip, WzEquip equipInfo, WzItemEnhancer enhancerInfo, out string message)
        {
            message = "";
            if (enhancerInfo.ItemId < 2046000)
            {
                if ((equip.ItemId - 1000000) / 10000 != (enhancerInfo.ItemId - 2040000) / 100)
                    return false;
            }
            else if (enhancerInfo.ItemId < 2047000)
            {
                int type = (enhancerInfo.ItemId - 2046000) / 100;
                switch (type)
                {
                    case 0:
                        if (!IsOneHandedWeaponScrollable(equip.ItemId))
                            return false;
                        break;
                    case 1:
                        if (!IsTwoHandedWeaponScrollable(equip.ItemId))
                            return false;
                        break;
                    case 2:
                        if (!IsArmorScrollable(equip.ItemId))
                            return false;
                        break;
                    case 3:
                        if (!IsAccessoryScrollable(equip.ItemId))
                            return false;
                        break;
                    default:
                        message = "Unhandled scroll, please report this on the forums. ItemId: " + enhancerInfo.ItemId;
                        return false;
                }
            }
            if (enhancerInfo.UseableOnIds.Count > 0 && !enhancerInfo.UseableOnIds.Contains(equip.ItemId))
                return false;
            int value;
            if (enhancerInfo.StatEnhancements.TryGetValue("reqCUC", out value) && equip.UpgradeCount < value)
            {
                message = "Item does not have the required upgrade count.";
                return false;
            }
            if ((enhancerInfo.StatEnhancements.TryGetValue("reqRUC", out value) || enhancerInfo.StatEnhancements.TryGetValue("tuc", out value)) && equip.RemainingUpgradeCount < value)
            {
                message = "Item does not have the required remaining upgrade count.";
                return false;
            }
            if (enhancerInfo.StatEnhancements.TryGetValue("reqEquipLevelMin", out value))
            {
                if (equipInfo.ReqLevel < value)
                {
                    message = "Item does not have the required level.";
                    return false;
                }
            }
            if (enhancerInfo.StatEnhancements.TryGetValue("reqEquipLevelMax", out value))
            {
                if (equipInfo.ReqLevel > value)
                {
                    message = "Item does not have the required level.";
                    return false;
                }
            }
            if (enhancerInfo.StatEnhancements.TryGetValue("recover", out value)) //clean slate
            {
                int totalPossibleUpgrades = equipInfo.TotalUpgradeCount + equip.HammersApplied;
                int failedUpgrades = totalPossibleUpgrades - equip.UpgradeCount - equip.RemainingUpgradeCount;
                if (failedUpgrades <= 0)
                {
                    message = "There are no upgrades to recover.";
                    return false;
                }
            }
            return true;
        }

        private static short GetNewChaosStat(short current, bool betterStats, bool noNegative)
        {
            int statRange = betterStats ? 10 : 5;
            bool add = noNegative || Functions.RandomBoolean(); //50% chance to subtract stat if it can be negative
            int statAdd = Functions.Random(0, statRange);
            if (!add) statAdd *= -1;
            return (short)Math.Max(0, current + statAdd);
        }

        private static bool IsTwoHandedWeaponScrollable(int itemId)
        {
            MapleItemType type = ItemConstants.GetMapleItemType(itemId);
            return type >= MapleItemType.TwoHandedSword && type <= MapleItemType.Fan;
        }

        private static bool IsOneHandedWeaponScrollable(int itemId)
        {
            if (!ItemConstants.IsWeapon(itemId)) return false;
            return !IsTwoHandedWeaponScrollable(itemId);
        }

        private static bool IsAccessoryScrollable(int itemId)
        {
            MapleItemType type = ItemConstants.GetMapleItemType(itemId);
            switch (type)
            {
                case MapleItemType.FaceAccessory:
                case MapleItemType.EyeAccessory:
                case MapleItemType.Earring:
                case MapleItemType.Ring:
                case MapleItemType.Pendant:
                case MapleItemType.Belt:
                case MapleItemType.Medal:
                    return true;
            }
            return false;
        }

        private static bool IsArmorScrollable(int itemId)
        {
            MapleItemType type = ItemConstants.GetMapleItemType(itemId);
            switch (type)
            {
                case MapleItemType.Cap:
                case MapleItemType.Top:
                case MapleItemType.Overall:
                case MapleItemType.Legs:
                case MapleItemType.Shoes:
                case MapleItemType.Glove:
                case MapleItemType.Cape:
                    return true;
            }
            return false;
        }

        public static class Packets
        {
            public static PacketWriter ShowScrollEffect(int characterId, byte scrollResult, int scrollItemId, int equipItemId, bool legendarySpirit = false)
            {
                var pw = new PacketWriter((ushort)SMSGHeader.SHOW_SCROLL_EFFECT);
                pw.WriteInt(characterId);
                pw.WriteByte(scrollResult);
                pw.WriteBool(legendarySpirit);

                pw.WriteInt(scrollItemId);
                pw.WriteInt(equipItemId);
                pw.WriteInt(1);

                pw.WriteShort(0);
                return pw;
            }
        }
    }

    public enum CubeType
    {
        None,
        Occult,
        MasterCraftsman,
        Meister,
        PlatinumMiracle
    }
}