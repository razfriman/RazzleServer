using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class BuffEntity
    {
        [Key]
        public int Id { get; set; }
        public long StartTime { get; set; }
        public int Length { get; set; }
        public int CharacterId { get; set; }
        public int SkillId { get; set; }
        public byte SkillLevel { get; set; }
        public byte Type { get; set; }
        public DateTime End { get; set; }
        public int Value { get; set; }
    }
}
