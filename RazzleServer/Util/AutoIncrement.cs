namespace RazzleServer.Util
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
                    int ret = current;
                    current++;
                    return ret;
                }
            }
        }
    }
}