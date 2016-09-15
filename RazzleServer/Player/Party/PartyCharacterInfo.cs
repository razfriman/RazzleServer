namespace RazzleServer.Party
{
    public class PartyCharacterInfo
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; }
        public int Job { get; private set; }
        public int Channel { get; set; }
        public int MapID { get; set; }

        public PartyCharacterInfo(int id, string name, int level, int job, int channel = -2, int mapId = 999999999)
        {
            ID = id;
            Name = name;
            Level = level;
            Job = job;
            Channel = channel;
            MapID = mapId;
        }
    }
}
