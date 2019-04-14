using RazzleServer.Common.Constants;
using RazzleServer.Common.Maple;
using RazzleServer.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Server.Maple
{
    public class BasicCharacterStats
    {
        public ICharacter BaseParent { get; set; }
        public virtual int BuddyListSlots { get; set; } = 20;
        public virtual Gender Gender { get; set; }
        public virtual byte Skin { get; set; }
        public virtual int Face { get; set; }
        public virtual int Hair { get; set; }
        public virtual byte Level { get; set; }
        public virtual Job Job { get; set; }
        public virtual short Strength { get; set; }
        public virtual short Dexterity { get; set; }
        public virtual short Intelligence { get; set; }
        public virtual short Luck { get; set; }
        public virtual short Health { get; set; }
        public virtual short MaxHealth { get; set; }
        public virtual short Mana { get; set; }
        public virtual short MaxMana { get; set; }
        public virtual short AbilityPoints { get; set; }
        public virtual short SkillPoints { get; set; }
        public virtual int Experience { get; set; }
        public virtual short Fame { get; set; }
        public virtual int Meso { get; set; }

        public BasicCharacterStats(ICharacter baseParent)
        {
            BaseParent = baseParent;
        }

        public virtual void Load(CharacterEntity character)
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

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteInt(BaseParent.Id);
            pw.WriteString(BaseParent.Name, 13);
            pw.WriteByte(Gender);
            pw.WriteByte(Skin);
            pw.WriteInt(Face);
            pw.WriteInt(Hair);
            pw.WriteLong(0); // Pet SN
            pw.WriteByte(Level);
            pw.WriteShort((short)Job);
            pw.WriteShort(Strength);
            pw.WriteShort(Dexterity);
            pw.WriteShort(Intelligence);
            pw.WriteShort(Luck);
            pw.WriteShort(Health);
            pw.WriteShort(MaxHealth);
            pw.WriteShort(Mana);
            pw.WriteShort(MaxMana);
            pw.WriteShort(AbilityPoints);
            pw.WriteShort(SkillPoints);
            pw.WriteInt(Experience);
            pw.WriteShort(Fame);
            pw.WriteInt(BaseParent.MapId);
            pw.WriteByte(BaseParent.SpawnPoint);
            pw.WriteLong(0);
            pw.WriteInt(0);
            pw.WriteInt(0);
            return pw.ToArray();
        }
    }
}
