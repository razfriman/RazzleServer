﻿using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class AccountStorageEntity
    {
        [Key] public int Id { get; set; }
        [Required] public int AccountId { get; set; }
        public int Meso { get; set; }
        public byte Slots { get; set; }

        public AccountEntity Account { get; set; }
    }
}
