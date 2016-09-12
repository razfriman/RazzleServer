namespace RazzleServer.Player
{
    public class Invite
    {
        public int SenderID;
        public InviteType Type;
        public Invite(int fromID, InviteType type)
        {
            Type = type;
            SenderID = fromID;
        }
    }
}
