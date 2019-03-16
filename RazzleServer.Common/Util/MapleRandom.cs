namespace RazzleServer.Common.Util
{
    public class MapleRandom
    {
        public int OriginalSeed1 { get; set; }

        public int OriginalSeed2 { get; set; }

        public int OriginalSeed3 { get; set; }

        private long _seed1;
        private long _seed2;
        private long _seed3;
        private long _oldSeed1;
        private long _oldSeed2;
        private long _oldSeed3;

        public MapleRandom()
        {
            OriginalSeed1 = Functions.Random();
            OriginalSeed2 = Functions.Random();
            OriginalSeed3 = Functions.Random();
            Seed(OriginalSeed1, OriginalSeed2, OriginalSeed3);
        }

        public long Random()
        {
            var seed1 = _seed1;
            var seed2 = _seed2;
            var seed3 = _seed3;

            _oldSeed1 = seed1;
            _oldSeed2 = seed2;
            _oldSeed3 = seed3;

            var newSeed1 = (seed1 << 12) ^ (seed1 >> 19) ^ ((seed1 >> 6) ^ (seed1 << 12)) & 0x1FFF;
            var newSeed2 = 16 * seed2 ^ (seed2 >> 25) ^ ((16 * seed2) ^ (seed2 >> 23)) & 0x7F;
            var newSeed3 = (seed3 >> 11) ^ (seed3 << 17) ^ ((seed3 >> 8) ^ (seed3 << 17)) & 0x1FFFFF;

            _seed1 = newSeed1;
            _seed2 = newSeed2;
            _seed3 = newSeed3;

            return (newSeed1 ^ newSeed2 ^ newSeed3) & 0xffffffffL;
        }

        public void Seed(int s1, int s2, int s3)
        {
            _seed1 = s1 | 0x100000;
            _oldSeed1 = s1 | 0x100000;

            _seed2 = s2 | 0x1000;
            _oldSeed2 = s2 | 0x1000;

            _seed3 = s3 | 0x10;
            _oldSeed3 = s3 | 0x10;
        }
    }
}
