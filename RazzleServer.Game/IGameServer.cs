namespace RazzleServer.Game
{
    public interface IGameServer : IMapleServer
    {
        byte ChannelId { get; set; }
        int Population { get; set; }
        AWorld World { get; set; }
        IServerManager Manager { get; set; }
    }
}
