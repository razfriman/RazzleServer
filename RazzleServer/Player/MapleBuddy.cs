namespace RazzleServer.Player
{
    public class MapleBuddy
    {
        public int CharacterID { get; set; }
        public int AccountID { get; set; }
        public string NickName { get; set; }
        public string Group { get; set; }
        public string Memo { get; set; }
        public bool IsRequest { get; set; }
        public int Channel { get; set; }
        public string Name { get; set; }
        public bool AccountBuddy { get; set; }

        public MapleBuddy(int characterId, int accountId, string name, string group, bool isRequest, string memo = "")
        {
            CharacterID = characterId;
            AccountID = accountId;
            NickName = name;
            Group = group;
            Memo = memo;
            IsRequest = isRequest;
            Channel = -1;
            AccountBuddy = accountId > 0;
            Name = string.Empty;
        }
    }
}