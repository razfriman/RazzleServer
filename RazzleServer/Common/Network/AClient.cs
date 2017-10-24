namespace MapleLib.PacketLib
{
    public abstract class AClient
    {
        public abstract void RecvPacket(PacketReader packet);

        public virtual void Disconnected()
        {

        }

        public virtual void Register()
        {

        }

        public virtual void Unregister()
        {

        }

        public virtual void Terminate()
        {

        }

        public abstract void Send(PacketWriter packet);

    }
}
