namespace RazzleServer.DB.Models
{
    public class Character
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public short Job { get; set; }
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Luk { get; set; }
        public short Int { get; set; }
        public short SP { get; set; }        
        public short AP { get; set; }
        public int GuildID { get; set; }
        public int Exp { get; set; }
        public int Mesos { get; set; }
        public int MapID { get; set; }
        public byte SpawnPoint { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }
        public short MaxHP { get; set; }
        public short MaxMP { get; set; }
        public short Fame { get; set; }
        public int Hair { get; set; }
        public int Face { get; set; }
        public int FaceMark { get; set; }
        public int GuildContribution { get; set; }
        public byte BuddyCapacity { get; set; }
        public byte Gender { get; set; }
        public byte Skin { get; set; }
        public byte GuildRank { get; set; }
        public byte AllianceRank { get; set; }
        public byte Fatigue { get; set; }
    }
}