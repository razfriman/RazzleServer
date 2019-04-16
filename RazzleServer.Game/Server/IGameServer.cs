namespace RazzleServer.Game.Server
{
    public interface IGameServer : IMapleServer
    {
        byte ChannelId { get; set; }
        int Population { get; set; }
        World World { get; set; }
        IServerManager Manager { get; set; }
    }
}
