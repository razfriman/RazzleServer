namespace RazzleServer.Server
{
    public interface IGameServer : IMapleServer
    {
        byte ChannelId { get; set; }
        int Population { get; set; }
        AWorld World { get; set; }
    }
}
