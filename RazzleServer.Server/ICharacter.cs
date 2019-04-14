using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Server
{
    public interface ICharacter
    {
        int Id { get; set; }

        int AccountId { get; set; }
        bool IsMaster { get; }
        byte WorldId { get; set; }
        string Name { get; set; }
        byte SpawnPoint { get; set; }
        byte Stance { get; set; }
        int MapId { get; set; }
        short Foothold { get; set; }
        byte Portals { get; set; }
        int Chair { get; set; }
        int Rank { get; set; }
        int RankMove { get; set; }
        int JobRank { get; set; }
        int JobRankMove { get; set; }

        void Send(PacketWriter packet);
        void LogCheatWarning(CheatType type);
    }
}
