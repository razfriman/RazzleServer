using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class ItemEntity
    {
        [Key] public int Id { get; set; }
        public int MapleId { get; set; }
        [Required] public int AccountId { get; set; }
        [Required] public int CharacterId { get; set; }
        public short Position { get; set; }
        public short Quantity { get; set; }
        public short Flags { get; set; }
        public bool IsStored { get; set; }
        public int? PetId { get; set; }
        public short Slot { get; set; }
        public DateTime Expiration { get; set; }
        public byte UpgradesAvailable { get; set; }
        public byte UpgradesApplied { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Luck { get; set; }
        public short Intelligence { get; set; }
        public short Health { get; set; }
        public short Mana { get; set; }
        public short WeaponAttack { get; set; }
        public short MagicAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Agility { get; set; }
        public byte PotentialState { get; set; }
        public short Potential1 { get; set; }
        public short Potential2 { get; set; }
        public short Potential3 { get; set; }
        public short BonusPotential1 { get; set; }
        public short BonusPotential2 { get; set; }
        public int Durability { get; set; }
        public short CustomExp { get; set; }
        public byte Enhancements { get; set; }
        public byte CustomLevel { get; set; }

        public AccountEntity Account { get; set; }
        public CharacterEntity Character { get; set; }
    }
}
