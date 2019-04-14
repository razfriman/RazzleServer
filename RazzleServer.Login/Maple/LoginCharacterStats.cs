using RazzleServer.Common.Constants;
using RazzleServer.Common.Maple;
using RazzleServer.Data;

namespace RazzleServer.Login.Maple
{
    public class LoginCharacterStats : ICharacterStats
    {
        public int BuddyListSlots { get; set; }
        public Gender Gender { get; set; }
        public byte Skin { get; set; }
        public int Face { get; set; }
        public int Hair { get; set; }
        public byte Level { get; set; }
        public Job Job { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Intelligence { get; set; }
        public short Luck { get; set; }
        public short Health { get; set; }
        public short MaxHealth { get; set; }
        public short Mana { get; set; }
        public short MaxMana { get; set; }
        public short AbilityPoints { get; set; }
        public short SkillPoints { get; set; }
        public int Experience { get; set; }
        public short Fame { get; set; }
        public int Meso { get; set; }

        public void Load(CharacterEntity character)
        {
            AbilityPoints = character.AbilityPoints;
            Dexterity = character.Dexterity;
            Experience = character.Experience;
            Face = character.Face;
            Fame = character.Fame;
            Hair = character.Hair;
            Health = character.Health;
            Intelligence = character.Intelligence;
            Job = (Job)character.Job;
            Level = character.Level;
            Luck = character.Luck;
            MaxHealth = character.MaxHealth;
            MaxMana = character.MaxMana;
            Meso = character.Meso;
            Mana = character.Mana;
            Skin = character.Skin;
            Strength = character.Strength;
            SkillPoints = character.SkillPoints;
            Strength = character.Strength;
            BuddyListSlots = character.BuddyListSlots;
            Gender = (Gender)character.Gender;
        }
    }
}
