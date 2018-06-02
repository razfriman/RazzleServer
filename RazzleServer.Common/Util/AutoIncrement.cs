namespace RazzleServer.Common.Util
{
    public class AutoIncrement
    {
        private readonly object _locker = new object();
        private int _current;

        public AutoIncrement(int startValue = 0)
        {
            _current = startValue;
        }

        public int Get
        {
            get
            {
                lock (_locker)
                {
                    var ret = _current;
                    _current++;
                    return ret;
                }
            }
        }
    }
}