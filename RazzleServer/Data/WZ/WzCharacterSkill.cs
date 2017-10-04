using Microsoft.Extensions.Logging;
using RazzleServer.Constants;
using RazzleServer.Handlers;
using RazzleServer.Map;
using RazzleServer.Map.Monster;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static RazzleServer.Data.WZ.WzMap;
using MapleLib.PacketLib;

namespace RazzleServer.Data.WZ
{
    public class WzCharacterSkill
    {
        public int SkillId { get; set; }
        public Dictionary<byte, SkillEffect> SkillEffects { get; set; }
        public byte MaxLevel { get; set; }
        public byte CombatOrdersMaxLevel { get; set; }
        public byte DefaultMastery { get; set; }
        public bool HasMastery { get; set; }
        public bool IsInvisible { get; set; }
        public bool IsBuff { get; set; }
        public bool IsPartySkill { get; set; }
        public bool IsHyperSkill { get; set; }
        public bool IsPassiveSkill { get; set; }
        public bool IsKeyDownSkill { get; set; }
        public bool HasFixedLevel { get; set; }
        public bool HasSummon { get; set; }
        public byte RequiredLevel { get; set; }
        public bool IsGmSkill => SkillId / 1000000 == 9;
        public Dictionary<int, byte> RequiredSkills { get; set; }
        public SummonAttackInfo SummonInfo { get; set; }
        public Point TopLeft { get; set; }
        public Point BottomRight { get; set; }

        public string Name { get; set; }

        public WzCharacterSkill()
        {
            SkillEffects = new Dictionary<byte, SkillEffect>();
            IsBuff = false;
            IsPartySkill = false;
            IsKeyDownSkill = false;
            IsInvisible = false;
            DefaultMastery = 0;
            HasFixedLevel = false;
            HasSummon = false;
        }

        public SkillEffect GetEffect(byte skillLevel)
        {
            SkillEffect ret;
            return SkillEffects.TryGetValue(skillLevel, out ret) ? ret : null;
        }

        public class SummonAttackInfo
        {            
            public byte MobCount { get; set; }
            public byte AttackCount { get; set; }           
            public int Delay { get; set; }
            public SummonMovementType MovementType { get; set; }
            public SummonType Type { get; set; }


            public SummonAttackInfo()
            {
                MobCount = 0;
                AttackCount = 0;
                Delay = 0;
            }
        }
    }

    public class SkillEffect
    {
        public WzCharacterSkill Parent;
        public byte Level { get; set; }
        public Dictionary<CharacterSkillStat, int> Info = new Dictionary<CharacterSkillStat,int>();
        public Dictionary<BuffStat, int> BuffInfo = new Dictionary<BuffStat,int>();
        public byte MobCount { get; set; }
        public byte AttackCount { get; set; }
        public int MpCon { get; set; }
        public List<MonsterBuffApplication> MonsterBuffs { get; private set; }

        public const int MAX_BUFF_TIME_S = 2000000;
        public const int MAX_BUFF_TIME_MS = 2000000000;

        private static ILogger Log = LogManager.Log;

        public SkillEffect(WzCharacterSkill parent, byte level)
        {
            Parent = parent;
            MpCon = 0;
            Level = level;
            MonsterBuffs = new List<MonsterBuffApplication>();
        }

        public static bool CheckAndApplySkillEffect(MapleCharacter chr, int skillId, WzCharacterSkill wzCharacterSkill, int skillLevel = -1, int numTargets = 0, int numAttacks = 0)
        {            
            if (skillLevel == -1)
                skillLevel = chr.GetSkillLevel(skillId);

            if (wzCharacterSkill == null)
            {
                wzCharacterSkill = DataBuffer.GetCharacterSkillById(skillId);
                if (wzCharacterSkill == null)
                    return false;
            }

            if (wzCharacterSkill.HasFixedLevel && JobConstants.JobCanLearnSkill(skillId, chr.Job))
                skillLevel = 1;

            if (skillLevel == 0 || (chr.HasSkillOnCooldown(skillId)))
            {
                Log.LogWarning($"Player tried using skill ${skillId} while level 0 or on cooldown.");
                return false;
            }

            SkillEffect effect = wzCharacterSkill.GetEffect((byte)skillLevel);
            if (effect == null) return false;

            bool shadowPartner = false;
            if (numTargets > 0)
            {
                int attackCount = effect.AttackCount;
                if ((chr.IsBandit && chr.HasBuff(ChiefBandit.SHADOW_PARTNER)) ||
                   (chr.IsAssassin && chr.HasBuff(Hermit.SHADOW_PARTNER)) ||
                   (chr.IsNightWalker && chr.HasBuff(NightWalker3.SHADOW_PARTNER)))
                {
                    attackCount *= 2;
                    shadowPartner = true;
                }
                if (effect.MobCount < numTargets || attackCount < numAttacks)
                    return false;
            }

            int bulletConsume;
            if (effect.Info.TryGetValue(CharacterSkillStat.bulletConsume, out bulletConsume))
            {
                if (shadowPartner)
                    bulletConsume *= 2; 
                if (!DealDamageHandler.HandleRangedAttackAmmoUsage(chr, bulletConsume))
                {
                    Log.LogWarning($"Character with job [{chr.Job}] tried using a skill with bulletCount [{bulletConsume}] but doesn't have the bullets!");
                    return false;
                }
            }

            if (chr.MP < effect.MpCon) return false;
            else chr.AddMP(-effect.MpCon);

            int hpCon;
            if (effect.Info.TryGetValue(CharacterSkillStat.hpCon, out hpCon))
            {
                if (chr.HP < hpCon) return false;
                chr.AddHP(-hpCon);
            }

            #region Manual skill handlers and checks
            switch (skillId)
            {
                case Berserker.EVIL_EYE_OF_DOMINATION:
                {
                    Buff evilEyeBuff = chr.GetBuff(Spearman.EVIL_EYE);
                    MapleSummon evilEye = chr.GetSummon(Spearman.EVIL_EYE);
                    if (evilEyeBuff == null || evilEye == null) 
                        return false;
                    int timeUsed = (int) DateTime.UtcNow.Subtract(evilEyeBuff.StartTime).TotalMilliseconds;
                    int timeRemainingMS = evilEyeBuff.Duration - timeUsed;
                    Buff newBuff = new Buff(Spearman.EVIL_EYE, effect, timeRemainingMS, chr);
                    if (evilEyeBuff.Stacks == Berserker.EVIL_EYE_OF_DOMINATION)
                    {
                        newBuff.Stacks = Spearman.EVIL_EYE;
                        evilEye.MovementType = SummonMovementType.Follow;
                    }
                    else
                    {
                        newBuff.Stacks = Berserker.EVIL_EYE_OF_DOMINATION;
                        evilEye.MovementType = SummonMovementType.CircleFollow;
                    }
                    chr.GiveBuff(newBuff);
                    return true; //no other actions needed
                }
                case Berserker.EVIL_EYE_SHOCK:
                {
                    MapleSummon evilEye = chr.GetSummon(Spearman.EVIL_EYE);
                    if (evilEye == null) 
                        return false;
                    List<MapleMonster> mobs = chr.Map.GetMobsInRange(new BoundingBox(evilEye.Position, wzCharacterSkill.TopLeft, wzCharacterSkill.BottomRight));
                    if (mobs.Count > 0)
                    {
                        int damage = (int)((effect.Info[CharacterSkillStat.damage] / 100.0) * chr.Stats.GetDamage());
                        int stunProp = effect.Info[CharacterSkillStat.prop];
                        int stunTime = effect.Info[CharacterSkillStat.time] * 1000;
                        int mobCounter = 0;
                        foreach (MapleMonster mob in mobs)
                        {
                            mob.Damage(chr, damage);
                            if (mob.Alive)
                            {
                                if (Functions.MakeChance(stunProp))
                                    mob.ApplyStatusEffect(skillId, MonsterBuffStat.STUN, 1, stunTime, chr);
                            }
                            mobCounter++;
                            if (mobCounter == 10)
                                break;
                        }
                    }
                    break;
                }
                case DarkKnight.SACRIFICE:
                    if (!chr.RemoveSummon(Spearman.EVIL_EYE))
                        return false;
                    chr.CancelBuff(Spearman.EVIL_EYE);
                    int healHpR = effect.Info[CharacterSkillStat.y];
                    int heal = (int)((healHpR / 100.0) * chr.Stats.MaxHp);
                    chr.AddHP(heal);
                    break;
            }
            #endregion

            #region Apply Cooldown

            bool skipCooldown = skillId == DarkKnight.GUNGNIRS_DESCENT && (chr.HasBuff(DarkKnight.SACRIFICE) || chr.HasBuff(DarkKnight.FINAL_PACT2));

            if (!skipCooldown)
            {
                int coolTime;
                if (effect.Info.TryGetValue(CharacterSkillStat.cooltime, out coolTime) && coolTime > 0)
                    chr.AddCooldown(skillId, (uint)coolTime * 1000); //time in the wz is in seconds
            }
            #endregion

            effect.ApplyEffect(chr, chr);

            if (wzCharacterSkill.IsBuff)
            {
                effect.ApplyBuffEffect(chr);
                chr.Map.BroadcastPacket(Packets.ShowForeignSkillEffect(chr.ID, chr.Level, skillId, effect.Level), chr);
            }

            if (wzCharacterSkill.IsPartySkill)
            {
                if (chr.Party != null) {
                    List<MapleCharacter> partyMembersOnSameMap = chr.Party.GetCharactersOnMap(chr.Map, chr.ID);
                    if (partyMembersOnSameMap.Count > 0)
                    {
                        List<MapleCharacter> partyMembersInRange = chr.Map.GetCharactersInRange(effect.CalculateBoundingBox(chr.Position, chr.IsFacingLeft), partyMembersOnSameMap);
                        foreach (MapleCharacter partyMember in partyMembersInRange)
                        {
                            effect.ApplyEffect(chr, partyMember);
                            if (wzCharacterSkill.IsBuff)
                                effect.ApplyBuffEffect(partyMember);
                        }
                    }
                }
                else if (wzCharacterSkill.IsGmSkill && chr.IsAdmin)
                {
                    var targets = chr.Map.GetCharactersInRange(effect.CalculateBoundingBox(chr.Position, chr.IsFacingLeft));
                    foreach (MapleCharacter target in targets.Where(x => x.ID != chr.ID))
                    {
                        effect.ApplyEffect(chr, target);
                        if (wzCharacterSkill.IsBuff)
                            effect.ApplyBuffEffect(target);
                    }
                }
            }
            return true;
        }

        public void ApplyEffect(MapleCharacter source, MapleCharacter target)
        {
            int value;
            if (SkillConstants.IsHealSkill(Parent.SkillId) && Info.TryGetValue(CharacterSkillStat.hp, out value))
            {
                int healHp = (int)((value / 100.0) * source.Stats.GetDamage());
                target.AddHP(healHp);
            }

            #region Mist
            if (SkillConstants.IsMistSkill(Parent.SkillId)) 
            {
                var sourcePos = source.Position;
                BoundingBox boundingBox = CalculateBoundingBox(sourcePos, source.IsFacingLeft);
                source.Map.SpawnMist(Parent.SkillId, Level, source, boundingBox, sourcePos, Info[CharacterSkillStat.time] * 1000, false);
            } 
            else if (Parent.SkillId == Priest.MYSTIC_DOOR)
            {
                MapleMap fromMap = source.Map;
                if (fromMap.MysticDoorLimit)                
                    return;                
                source.CancelDoor(Priest.MYSTIC_DOOR);
                int partyId = source.Party?.ID ?? -1;
                Point fromMapPosition = source.Map.GetDropPositionBelow(source.Position, source.Position);
                int time = Info[CharacterSkillStat.time] * 1000;
                
                MapleMap toMap = ServerManager.GetChannelServer(source.Client.Channel).GetMap(source.Map.ReturnMap);
                if (toMap != null)
                {                    
                    Portal toPortal = toMap.TownPortal;
                    if (toPortal != null)
                    {
                        Point toMapPosition = toMap.GetDropPositionBelow(toPortal.Position, toPortal.Position);
                        MysticDoor sourceDoor = new MysticDoor(Parent.SkillId, source, fromMapPosition, fromMap, toMap, toPortal, time, true);                        
                        fromMap.SpawnStaticObject(sourceDoor);
                        source.AddDoor(sourceDoor);                        
                    }
                }
            }
            #endregion

            #region Summons
            if (Parent.HasSummon)
            {
                source.RemoveSummon(Parent.SkillId);  //Remove old one if exists
                WzCharacterSkill.SummonAttackInfo info = Parent.SummonInfo;
                MapleSummon summon = new MapleSummon(source.Map.GetNewObjectID(), Parent.SkillId, source.Position, info.Type, info.MovementType, source, Level, Info[CharacterSkillStat.time] * 1000);
                source.AddSummon(summon);
            }
            #endregion

            //Custom handling:
            switch (Parent.SkillId)
            {
                case Priest.HOLY_MAGIC_SHELL:
                case SuperGameMaster.HEAL_DISPEL:
                    target.AddHP(target.Stats.MaxHp);
                    break;
                case Bishop.RESURRECTION:
                case SuperGameMaster.RESURRECTION:
                
                    if (target.IsDead) 
                    {
                        target.Revive(false, false, true);
                    }
                    break;
            }
        }

        public void ApplyBuffEffect(MapleCharacter target)
        {
            if (Parent.SkillId == Priest.HOLY_MAGIC_SHELL && target.HasSkillOnCooldown(Priest.HOLY_MAGIC_SHELL + 1000))
                return;

            int time;
            if (Info.TryGetValue(CharacterSkillStat.time, out time))
            {
                ulong ltime = 1000 * (ulong)time;
                if (ltime >= int.MaxValue)
                    ltime = MAX_BUFF_TIME_MS;
                int uTime = (int)Math.Min(((target.Stats.BuffTimeR / 100.0) * ltime), MAX_BUFF_TIME_MS);
                Buff buff = new Buff(Parent.SkillId, this, uTime, target, DateTime.UtcNow);
                target.GiveBuff(buff);               
            }
            else
            {
                Log.LogError($"Buff from skill [{Parent.SkillId}] has no buff time");
            }
        }

        public BoundingBox CalculateBoundingBox(Point origin, bool facingLeft)
        {  
            int range;
            if (Info.TryGetValue(CharacterSkillStat.range, out range))
            {                    
                if (facingLeft)
                    return new BoundingBox(origin.X - range, origin.Y - 100, origin.X, origin.Y);
                else
                    return new BoundingBox(origin.X, origin.Y - 100, origin.X + range, origin.Y);
            }
            else
                Log.LogError($"Skill [{Parent.SkillId}] is a Party ability but has no bounding box or range attributes");            
            return new BoundingBox();
        }
        
        #region Load Buffstats
        public void LoadBuffStats()
        {
            BuffInfo = new Dictionary<BuffStat, int>();

            CheckAndAddBuffStat(MapleBuffStat.WATK, CharacterSkillStat.pad);
            CheckAndAddBuffStat(MapleBuffStat.WDEF, CharacterSkillStat.pdd);
            CheckAndAddBuffStat(MapleBuffStat.MATK, CharacterSkillStat.mad);
            CheckAndAddBuffStat(MapleBuffStat.MDEF, CharacterSkillStat.mdd);
            CheckAndAddBuffStat(MapleBuffStat.ACC, CharacterSkillStat.acc);
            CheckAndAddBuffStat(MapleBuffStat.AVOID, CharacterSkillStat.eva);
            CheckAndAddBuffStat(MapleBuffStat.SPEED, CharacterSkillStat.speed);
            CheckAndAddBuffStat(MapleBuffStat.JUMP, CharacterSkillStat.jump);
            CheckAndAddBuffStat(MapleBuffStat.MAXHP_R, CharacterSkillStat.mhpR);
            CheckAndAddBuffStat(MapleBuffStat.MAXMP_R, CharacterSkillStat.mmpR);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_MAXHP, CharacterSkillStat.indieMhp);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_MAXMP, CharacterSkillStat.indieMmp);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_MAXHP_R, CharacterSkillStat.indieMhpR);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_MAXMP_R, CharacterSkillStat.indieMmpR);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_JUMP, CharacterSkillStat.indieJump);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_SPEED, CharacterSkillStat.indieSpeed);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_ACC, CharacterSkillStat.indieAcc);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_AVOID, CharacterSkillStat.indieEva);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_STATS, CharacterSkillStat.indieAllStat);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_WDEF, CharacterSkillStat.indiePdd);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_MDEF, CharacterSkillStat.indieMdd);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_CRIT, CharacterSkillStat.indieCr);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_BOSS, CharacterSkillStat.indieBDR);
            CheckAndAddBuffStat(MapleBuffStat.STACKING_DMG_R, CharacterSkillStat.indieDamR);
            
                       
            #region Manual buffstats
            switch (Parent.SkillId)
            {                   
                #region Boosters
                case Fighter.WEAPON_BOOSTER:
                case Page.WEAPON_BOOSTER:
                case Spearman.WEAPON_BOOSTER:
                case IceLightning2.MAGIC_BOOSTER:
                case Cleric.MAGIC_BOOSTER:
                case FirePoison2.MAGIC_BOOSTER:
                case Hunter.BOW_BOOSTER:
                case Crossbowman.CROSSBOW_BOOSTER:
                case Assassin.CLAW_BOOSTER:
                case Bandit.DAGGER_BOOSTER:
                case DualBlade2p.KATARA_BOOSTER:
                case Jett1.GALACTIC_MIGHT:
                case Brawler.KNUCKLE_BOOSTER:
                case Gunslinger.GUN_BOOSTER:
                case Cannoneer2.CANNON_BOOSTER:
                case Evan6.MAGIC_BOOSTER:
                case Mercedes2.DUAL_BOWGUNS_BOOST:
                case Phantom2.CANE_BOOSTER:
                case DemonSlayer1.BATTLE_PACT:
                case BattleMage2.STAFF_BOOST:
                case WildHunter1.CROSSBOW_BOOSTER:
                case Mechanic2.MECHANIC_RAGE:
                case Xenon2.XENON_BOOSTER:
                case Hayato2.KATANA_BOOSTER:
                case Kanna2.RADIANT_PEACOCK:
                case Mihile2.SWORD_BOOSTER:
                case Kaiser2.BLAZE_ON:
                    BuffInfo.Add(MapleBuffStat.STACKING_BOOSTER, Info[CharacterSkillStat.x]);
                    break;
                #endregion

                #region MapleWarrior
                case Hero.MAPLE_WARRIOR:
                case Paladin.MAPLE_WARRIOR:
                case DarkKnight.MAPLE_WARRIOR:
                case FirePoison4.MAPLE_WARRIOR:
                case IceLightning4.MAPLE_WARRIOR:                
                case Bishop.MAPLE_WARRIOR:                   
                case Bowmaster.MAPLE_WARRIOR:
                case Marksman.MAPLE_WARRIOR:
                case Shadower.MAPLE_WARRIOR:
                case NightLord.MAPLE_WARRIOR:
                case DualBlade4.MAPLE_WARRIOR:
                case Corsair.MAPLE_WARRIOR:
                case Buccaneer.MAPLE_WARRIOR:
                case Cannoneer4.MAPLE_WARRIOR:
                case Jett4.MAPLE_WARRIOR:
                case Evan9.MAPLE_WARRIOR:
                case Mercedes4.MAPLE_WARRIOR:
                case Phantom4.MAPLE_WARRIOR:
                case DemonSlayer4.MAPLE_WARRIOR:
                case DemonAvenger4.MAPLE_WARRIOR:
                case BattleMage4.MAPLE_WARRIOR:
                case WildHunter4.MAPLE_WARRIOR:
                case Mechanic4.MAPLE_WARRIOR:
                case Xenon4.MAPLE_WARRIOR:
                case Hayato4.AKATSUKI_HERO:
                case Kanna4.DAWNS_WARRIOR_MAPLE_WARRIOR:
                case Mihile4.MAPLE_WARRIOR:
                case Kaiser4.NOVA_WARRIOR:
                case AngelicBuster4.NOVA_WARRIOR:
                    //BuffInfo.Add(MapleBuffStat.MAPLE_WARRIOR, Info[CharacterSkillStat.x]);
                    break;
                #endregion
                case Swordman.IRON_BODY:
                    BuffInfo.Remove(MapleBuffStat.MAXHP_R);                    
                    break;
                case Spearman.HYPER_BODY:
                    BuffInfo.Add(MapleBuffStat.MAXHP_R, Info[CharacterSkillStat.x]);
                    BuffInfo.Add(MapleBuffStat.MAXMP_R, Info[CharacterSkillStat.y]);
                    break;
                case Spearman.EVIL_EYE:
                case Berserker.EVIL_EYE_OF_DOMINATION:
                    //BuffInfo.Add(MapleBuffStat.EVIL_EYE, 1);
                    break;
                case Berserker.EVIL_EYE_SHOCK: //correct?
                    MonsterBuffs.Add(new MonsterBuffApplication(MonsterBuffStat.STUN, 1, Info[CharacterSkillStat.prop], Info[CharacterSkillStat.time] * 1000));
                    break;
                case Berserker.CROSS_SURGE:
                    //BuffInfo.Add(MapleBuffStat.CROSS_SURGE, 1);
                    break;
                case DarkKnight.SACRIFICE:
                    //BuffInfo.Add(MapleBuffStat.IGNORE_DEFENSE_R, Info[CharacterSkillStat.x]);
                    break;
                case DarkKnight.FINAL_PACT2:
                    //BuffInfo.Add(MapleBuffStat.FINAL_PACT1, 1);
                    //BuffInfo.Add(MapleBuffStat.FINAL_PACT2, 1);
                    //BuffInfo.Add(MapleBuffStat.FINAL_PACT3, 1);
                    break;
                case Magician.MAGIC_GUARD:
                    BuffInfo.Add(MapleBuffStat.MAGIC_GUARD, Info[CharacterSkillStat.x]);
                    break;
                case FirePoison3.TELEPORT_MASTERY:
                case IceLightning3.TELEPORT_MASTERY:
                case Priest.TELEPORT_MASTERY:
                case BlazeWizard3.TELEPORT_MASTERY:
                case Evan8.TELEPORT_MASTERY:
                case BattleMage3.TELEPORT_MASTERY:
                    //BuffInfo.Add(MapleBuffStat.TELEPORT_MASTERY, Info[CharacterSkillStat.x]);
                    Info[CharacterSkillStat.time] = SkillEffect.MAX_BUFF_TIME_S;
                    break;
                case IceLightning4.CHAIN_LIGHTNING:
                    var clbuff = new MonsterBuffApplication(Info[CharacterSkillStat.prop], Info[CharacterSkillStat.time] * 1000);
                    clbuff.AddBuff(MonsterBuffStat.STUN);
                    MonsterBuffs.Add(clbuff);
                    break;
                case FirePoison4.ARCANE_AIM:
                case IceLightning4.ARCANE_AIM:
                case Bishop.ARCANE_AIM:
                    //BuffInfo.Add(MapleBuffStat.ARCANE_AIM, 1);
                    break;
                case Cleric.BLESS:
                    //BuffInfo.Add(MapleBuffStat.BLESS, Level);
                    break;
                case Cleric.BLESSED_ENSEMBLE:
                    //BuffInfo.Add(MapleBuffStat.BLESSED_ENSEMBLE, 1);
                    break;
                case Cleric.INVINCIBLE:
                    //BuffInfo.Add(MapleBuffStat.INVINCIBLE, Info[CharacterSkillStat.x]);
                    break;
                case Priest.HOLY_SYMBOL:
                    //BuffInfo.Add(MapleBuffStat.HOLY_SYMBOL, Info[CharacterSkillStat.x]);
                    break;
                case Priest.HOLY_MAGIC_SHELL:
                    //BuffInfo.Add(MapleBuffStat.HOLY_MAGIC_SHELL, Info[CharacterSkillStat.x]);
                    break;
                case Priest.DIVINE_PROTECTION:
                    //BuffInfo.Add(MapleBuffStat.DIVINE_PROTECTION, 1);
                    break;
                case Bishop.INFINITY:
                    //BuffInfo.Add(MapleBuffStat.POWER_STANCE, Info[CharacterSkillStat.prop]);
                    //BuffInfo.Add(MapleBuffStat.INFINITY, Info[CharacterSkillStat.z]);
                    break;                
                case Bishop.ADVANCED_BLESSING:
                    //BuffInfo.Add(MapleBuffStat.ADVANCED_BLESSING, Level);
                    break;
                case Thief.DARK_SIGHT:
                case NightWalker1.DARK_SIGHT:          
                    BuffInfo.Add(MapleBuffStat.DARK_SIGHT, 1);
                    break;  
                case Hermit.SHADOW_PARTNER:
                case ChiefBandit.SHADOW_PARTNER:
                case NightWalker3.SHADOW_PARTNER:
                    //BuffInfo.Add(MapleBuffStat.SHADOW_PARTNER, Info[CharacterSkillStat.x]);
                    break;
                case Bandit.CRITICAL_GROWTH:
                    //BuffInfo.Add(MapleBuffStat.CRITICAL_GROWTH, 1);
                    break;
                case Bandit.MESOGUARD:
                    //BuffInfo.Add(MapleBuffStat.MESOGUARD, Info[CharacterSkillStat.x]);
                    break;
                case ChiefBandit.PICKPOCKET:
                    //BuffInfo.Add(MapleBuffStat.PICKPOCKET, Info[CharacterSkillStat.x]);
                    break;
                case WildHunter2.CALL_OF_THE_WILD:
                    //BuffInfo.Add(MapleBuffStat.ATTACK_PERCENT, Info[CharacterSkillStat.z]);
                    //BuffInfo.Add(MapleBuffStat.CRIT, Info[CharacterSkillStat.y]);
                    //BuffInfo.Add(MapleBuffStat.DMG_REDUCTION_R, Info[CharacterSkillStat.z]);
                    //BuffInfo.Add(MapleBuffStat.AVOID_PERCENT, Info[CharacterSkillStat.z]);
                    //BuffInfo.Add(MapleBuffStat.MAX_MP_PERCENT, Info[CharacterSkillStat.z]);
                    break;

            }
            #endregion

            BuffInfo = BuffInfo.OrderBy(x => x.Key.BitIndex).ToDictionary(x => x.Key, x => x.Value);
        }

        void CheckAndAddBuffStat(BuffStat buff, CharacterSkillStat stat)
        {
            int val;
            if (Info.TryGetValue(stat, out val))
            {
                BuffInfo.Add(buff, val);
            }
        }
        #endregion

        public static class Packets
        {
            public static PacketWriter ShowForeignSkillEffect(int characterId, byte characterLevel, int skillId, byte skillLevel)
            {
                var pw = new PacketWriter((ushort)SMSGHeader.SHOW_SKILL_EFFECT);
                pw.WriteInt(characterId);
                pw.WriteByte(1);
                pw.WriteInt(skillId);
                pw.WriteByte(characterLevel);
                pw.WriteByte(skillLevel);
                return pw;
            }
        }
    }

    public class MonsterBuffApplication
    {
        public Dictionary<BuffStat, int> Buffs { get; private set; }
        public int Prop { get; private set; }
        public int Duration { get; private set; }

        public MonsterBuffApplication(int prop, int durationMS)
        {
            Buffs = new Dictionary<BuffStat, int>();
            Prop = prop;
            Duration = durationMS;
        }

        public MonsterBuffApplication(BuffStat buffStat, int buffValue, int prop, int durationMS)
        {
            Buffs = new Dictionary<BuffStat, int>() { { buffStat, buffValue } };
            Prop = prop;
            Duration = durationMS;
        }     

        public void AddBuff(BuffStat buffStat, int buffValue = 1)
        {
            Buffs.Add(buffStat, buffValue);
        }
    }


    public enum CharacterSkillStat : int
    {
        unset,
        abnormalDamR, //Additional Damage on Targets with Abnormal Status
        acc, //Increase Accuracy +
        acc2dam, //Weapon Accuracy or Magic Accuracy (higher) to Damage
        acc2mp, //Accuracy to Max MP
        accR, //Accuracy %
        accX, //Accuracy + 
        ar, //Accuracy %
        asrR, //Abnormal Status Resistance % 
        attackCount, //Number of attacks, similiar to bulletCount
        bdR, //Damage on Bosses %
        bufftimeR, //Buff Skill duration increase %
        bulletConsume, //Consume bullets
        bulletCount, //Number of attacks hit
        coolTimeR, //Reduce Skill cooldown %
        cooltime, //Cooldown time
        costmpR, //mp cost %
        cr, //Critical % 
        criticaldamageMax, //Critical Maximum Damage Increase +
        criticaldamageMin, //Minimum Critical Damage Increase +
        damR, //Damage %
        damage, //Damage %
        damagepc, //Rage of Pharaoh and Bamboo Rain has this, drop from sky?
        dateExpire, //Useless date stuffs
        dex, //Increase DEX + 
        dex2str, //DEX to STR
        dexFX, //Increase DEX
        dexX, //Increase DEX
        dot, //Damage over time %
        dotInterval, //Damage dealt at intervals
        dotSuperpos, //Damage over time stack
        dotTime, //DOT time length (Lasts how long)
        dropR, //Increases Drop %
        emad, //Enhanced Magic ATT
        emdd, //Enhanced Magic DEF
        emhp, //Enhanced HP
        emmp, //Enhanced MP
        epad, //Enhanced ATT
        epdd, //Enhanced DEF
        er, //Avoidability %
        eva, //Avoidability Increase, avoid
        eva2hp, //Convert Avoidability to HP
        evaR, //Avoidability %
        evaX, //Avoidability Increase
        expLossReduceR, //Reduce EXP loss at death %
        expR, //Additional % EXP
        extendPrice, //[Guild] Extend price
        finalAttackDamR, //Additional damage from Final Attack skills %
        fixdamage, //Fixed damage dealt upon using skill
        forceCon, //Fury Cost
        hcCooltime,
        hcHp,
        hcProp,
        hcReflect,
        hcSubProp,
        hcSubTime,
        hcTime,
        hp, //Restore HP/Heal
        hpCon, //HP Consumed
        iceGageCon, //Ice Gauge Cost
        ignoreMobDamR, //Ignore Mob Damage to Player %
        ignoreMobpdpR, //Ignore Mob DEF % -> Attack higher
        indieAcc, //Accuracy +
        indieAllStat, //All Stats +
        indieBDR, //Boss dmg
        indieCr, //crit %
        indieDamR, //Damage Increase %
        indieEva, //Avoidability +
        indieJump, //Jump Increase +
        indieMad, //Magic Damage Increase
        indieMaxDamageOver,
        indieMhp, //Max HP Increase +
        indieMhpR, //Max HP Increase %
        indieMmp, //Max MP Increase +
        indieMmpR, //Max MP Increase %
        indiePad, //Damage Increase
        indieSpeed, //Speed +
        indiePdd, //flat physical damage
        indieMdd, //flat magic damage
        indieMDF, //Force ?
        int_,  //Increase INT
        int2luk, //Convert INT to LUK
        intFX, //Increase INT
        intX, //Increase INT
        itemCon, //Consumes item upon using <itemid>
        itemConNo, //amount for above
        itemConsume, //Uses certain item to cast that attack, the itemid doesn't need to be in inventory, just the effect.
        jump, //Jump Increase
        kp, //Body count attack stuffs           
        luk, //Increase LUK
        luk2dex, //Convert LUK to DEX
        lukFX, //Increase LUK
        lukX, //Increase LUK
        lv2mhp, //max hp per level
        lv2mmp, //max mp per level
        lv2damX, //Additional damage per character level
        lv2mad, //Additional magic damage per character level
        lv2mdX, //Additional magic defense per character level
        lv2pad, //Additional damage per character level
        lv2pdX, //Additional defense per character level
        mad, //Magic ATT +
        madX, //Magic ATT +
        mastery, //Increase mastery
        maxLevel, //Max Skill level
        MDamageOver,
        MDF,
        mdd, //Magic DEF
        mdd2dam, //Magic DEF to Damage
        mdd2pdd, //Magic DEF to Weapon DEF
        mdd2pdx, //When hit with a physical attack, damage equal to #mdd2pdx% of Magic DEF is ignored
        mddR, //Magic DEF %
        mddX, //Magic DEF
        mesoR, //Mesos obtained + %
        mhp2damX, //Max HP added as additional damage
        mhpR, //Max HP %
        mhpX, //Max HP +
        minionDeathProp, //Instant kill on normal monsters in Azwan Mode
        mmp2damX, //Max MP added as additional damage
        mmpR, //Max MP %
        mmpX, //Max MP +
        mobCount, //Max Enemies hit
        morph, //MORPH ID
        mp, //Restore MP/Heal
        mpCon, //MP Cost
        mpConEff, //MP Potion effect increase %
        mpConReduce, //MP Consumed reduce
        nbdR, //Increases damage by a set percentage when attacking a normal monster.
        nocoolProp, //When using a skill, Cooldown is not applied at #nocoolProp% chance. Has no effect on skills without Cooldown.
        onHitHpRecoveryR, //Chance to recover HP when attacking.
        onHitMpRecoveryR, //Chance to recover MP when attacking.
        pad, //Attack +
        padX, //Attack + 
        passivePlus, //Increases level of passive skill by #
        pdd, //Weapon DEF
        pdd2dam, //Weapon DEF added as additional damage
        pdd2mdd, //Weapon DEF added to Magic DEF
        pdd2mdx, //When hit with a magical attack, damage equal to #pdd2mdx% of Weapon DEF is ignored
        pddR, //Weapon DEF %
        pddX, //Weapon DEF
        pdR, //Damage % increase
        period, //[Guild/Professions] time taken
        price, //[Guild] price to purchase
        priceUnit, //[Guild] Price stuffs
        prop, //Percentage chance over 100%
        psdJump, //Passive Jump Increase
        psdSpeed, //Passive Speed Increase
        PVPdamage, //PVP damage
        range, //Range         
        reqGuildLevel, //[Guild] guild req level
        selfDestruction, //Self Destruct Damage
        speed, //Increase moving speed
        speedMax, //Max Movement Speed +
        str, //Increase STR
        str2dex, //STR to DEX
        strFX, //Increase STR
        strX, //Increase STR
        subProp, //Summon Damage Prop
        subTime, //Summon Damage Effect time
        suddenDeathR, //Instant kill on enemy %
        summonTimeR, //Summon Duration + %
        targetPlus, //Increases the number of enemies hit by multi-target skill
        tdR, //Increases damage by a set percentage when attacking a tower
        terR, //Elemental Resistance %
        time, //bufflength
        s,
        t, //Damage taken reduce
        u,
        v,
        w,
        x,
        y,
        z
    }
}
