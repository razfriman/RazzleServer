using System.Collections.Generic;
using RazzleServer.Common.Constants;

namespace RazzleServer.DataProvider.Cache
{
    public class CachedBuffs
    {
        public readonly Dictionary<int, BuffValueTypes> Data = new Dictionary<int, BuffValueTypes>
        {
            {(int)SkillNames.Fighter.AxeBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Fighter.SwordBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Page.BwBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Page.SwordBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Spearman.SpearBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Spearman.PolearmBooster, BuffValueTypes.Booster},
            {(int)SkillNames.FirePoisonMage.SpellBooster, BuffValueTypes.Booster},
            {(int)SkillNames.IceLightningMage.SpellBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Hunter.BowBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Crossbowman.CrossbowBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Assassin.ClawBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Bandit.DaggerBooster, BuffValueTypes.Booster},
            {(int)SkillNames.Magician.MagicGuard, BuffValueTypes.MagicGuard},
            {(int)SkillNames.Magician.MagicArmor, BuffValueTypes.WeaponDefense},
            {(int)SkillNames.Swordsman.IronBody, BuffValueTypes.WeaponDefense},
            {(int)SkillNames.Archer.Focus, BuffValueTypes.Accuracy | BuffValueTypes.Avoidability},
            {(int)SkillNames.Fighter.Rage, BuffValueTypes.WeaponAttack | BuffValueTypes.WeaponDefense},
            {(int)SkillNames.Fighter.PowerGuard, BuffValueTypes.PowerGuard},
            {(int)SkillNames.Page.PowerGuard, BuffValueTypes.PowerGuard},
            {(int)SkillNames.Spearman.IronWill, BuffValueTypes.WeaponDefense | BuffValueTypes.MagicDefense},
            {(int)SkillNames.Spearman.HyperBody, BuffValueTypes.MaxHealth | BuffValueTypes.MaxMana},
            {(int)SkillNames.FirePoisonWizard.Meditation, BuffValueTypes.MagicAttack},
            {(int)SkillNames.IceLightningWizard.Meditation, BuffValueTypes.MagicAttack},
            {(int)SkillNames.Cleric.Invincible, BuffValueTypes.Invincible},
            {
                (int)SkillNames.Cleric.Bless, BuffValueTypes.WeaponDefense | BuffValueTypes.MagicDefense |
                                              BuffValueTypes.Accuracy |
                                              BuffValueTypes.Avoidability
            },
            {
                (int)SkillNames.Gm.Bless, BuffValueTypes.WeaponAttack | BuffValueTypes.WeaponDefense |
                                          BuffValueTypes.MagicAttack |
                                          BuffValueTypes.MagicDefense | BuffValueTypes.Accuracy |
                                          BuffValueTypes.Avoidability
            },
            {(int)SkillNames.ChiefBandit.MesoGuard, BuffValueTypes.MesoGuard},
            {(int)SkillNames.Priest.HolySymbol, BuffValueTypes.HolySymbol},
            {(int)SkillNames.Gm.HolySymbol, BuffValueTypes.HolySymbol},
            {(int)SkillNames.ChiefBandit.Pickpocket, BuffValueTypes.PickPocketMesoUp},
            {(int)SkillNames.Hermit.MesoUp, BuffValueTypes.PickPocketMesoUp},
            {(int)SkillNames.DragonKnight.DragonRoar, BuffValueTypes.Stun},
            {(int)SkillNames.WhiteKnight.BwFireCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.WhiteKnight.BwIceCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.WhiteKnight.BwLitCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.WhiteKnight.SwordFireCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.WhiteKnight.SwordIceCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.WhiteKnight.SwordLitCharge, BuffValueTypes.MagicAttack | BuffValueTypes.Charges},
            {(int)SkillNames.Assassin.Haste, BuffValueTypes.Speed | BuffValueTypes.Jump},
            {(int)SkillNames.Bandit.Haste, BuffValueTypes.Speed | BuffValueTypes.Jump},
            {(int)SkillNames.Gm.Haste, BuffValueTypes.Speed | BuffValueTypes.Jump},
            {(int)SkillNames.Rogue.DarkSight, BuffValueTypes.Speed | BuffValueTypes.DarkSight},
            {(int)SkillNames.Gm.Hide, BuffValueTypes.Invincible | BuffValueTypes.DarkSight},
            {(int)SkillNames.Hunter.SoulArrow, BuffValueTypes.SoulArrow},
            {(int)SkillNames.Crossbowman.SoulArrow, BuffValueTypes.SoulArrow},
            {(int)SkillNames.Hermit.ShadowPartner, BuffValueTypes.ShadowPartner},
            {(int)SkillNames.Gm.ShadowPartner, BuffValueTypes.ShadowPartner},
            {(int)SkillNames.Crusader.ComboAttack, BuffValueTypes.ComboAttack},
            {(int)SkillNames.DragonKnight.DragonBlood, BuffValueTypes.WeaponAttack | BuffValueTypes.DragonBlood}
        };
    }
}
