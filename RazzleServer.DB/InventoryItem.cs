using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class InventoryItem
    {
        public long ID { get; set; }
        public int ItemID { get; set; }
        public int CharacterID { get; set; }
        public short Position { get; set; }
        public short Quantity { get; set; }
        public short Flags { get; set; }
        [MaxLength(13)]
        public string Creator { get; set; }
        public string Source { get; set; }
    }
}
