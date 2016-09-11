﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class Buddy
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public int BuddyCharacterID { get; set; }
        public int AccountID { get; set; }
        public int BuddyAccountID { get; set; }
        public bool IsRequest { get; set; }

        [MaxLength(13)]
        public string Name { get; set; }
        [MaxLength(16)]
        public string Group { get; set; }
        [MaxLength(256)]
        public string Memo { get; set; }
    }
}
