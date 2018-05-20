namespace RazzleServer.Common.Util
{
    public class AutoIncrement
    {
        private readonly object locker = new object();
        private int current;

        public AutoIncrement(int startValue = 0)
        {
            current = startValue;
        }

        public int Get
        {
            get
            {
                lock (locker)
                {
                    var ret = current;
                    current++;
                    return ret;
                }
            }
        }
    }
}