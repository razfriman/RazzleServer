namespace RazzleServer.Common.Data
{
    public interface IMapleSavable
    {
        int Create();
        void Save();
        void Load(object key);
    }
}
