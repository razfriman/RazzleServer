using RazzleServer.Common.Constants;

namespace RazzleServer.Common.Maple
{
    public interface ICharacterStats
    {
        int BuddyListSlots { get; set; }
        Gender Gender { get; set; }
        byte Skin { get; set; }
        int Face { get; set; }
        int Hair { get; set; }
        byte Level { get; set; }
        Job Job { get; set; }
        short Strength { get; set; }
        short Dexterity { get; set; }
        short Intelligence { get; set; }
        short Luck { get; set; }
        short Health { get; set; }
        short MaxHealth { get; set; }
        short Mana { get; set; }
        short MaxMana { get; set; }
        short AbilityPoints { get; set; }
        short SkillPoints { get; set; }
        int Experience { get; set; }
        short Fame { get; set; }
        int Meso { get; set; }
    }
}
