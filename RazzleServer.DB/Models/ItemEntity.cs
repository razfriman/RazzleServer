using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.DB.Models
{
    public class ItemEntity
    {
        public int ID { get; set; }
        public int MapleID { get; set; }
        public int AccountID { get; set; }
        public int CharacterID { get; set; }
        public short Position { get; set; }
        public short Quantity { get; set; }
        public short Flags { get; set; }
        [MaxLength(13)]
        public string Creator { get; set; }
        public string Source { get; set; }
        public bool IsStored { get; set; }
        public int? PetID { get; set; }
        public byte Slot { get; set; }
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
    }
}