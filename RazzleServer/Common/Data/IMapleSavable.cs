namespace RazzleServer.Common.Data
{
    public interface IMapleSavable
    {
        void Create();
        void Save();
        void Load(object key);
    }
}
