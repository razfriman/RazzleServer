namespace RazzleServer.Player
{
    public class MapleMessengerCharacter
    {
        public int Position { get; }
        public MapleCharacter Character { get; set; }

        public MapleMessengerCharacter(int position, MapleCharacter chr)
        {
            Position = position;
            Character = chr;
        }
    }
}