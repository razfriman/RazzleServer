using System;
namespace RazzleServer.Common.Data
{
    public class MapleSavable<T> where T : class
    {
        public virtual T Create()
        {
            return null;
        }

        public virtual void Save()
        {
        }

        public virtual void LoadByKey(int key)
        {

        }

        public virtual void LoadByKey(string key)
        {

        }
    }
}
