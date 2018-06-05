namespace RazzleServer.Common.Util
{
    public class MapleRandom
    {
        public int OriginalSeed1 { get; set; }

        public int OriginalSeed2 { get; set; }

        public int OriginalSeed3 { get; set; }

        private long seed1;
        private long seed2;
        private long seed3;
        private long oldSeed1;
        private long oldSeed2;
        private long oldSeed3;

        public MapleRandom()
        {
            OriginalSeed1 = Functions.Random();
            OriginalSeed2 = Functions.Random();
            OriginalSeed3 = Functions.Random();
            Seed(OriginalSeed1, OriginalSeed2, OriginalSeed3);
        }

        public long Random()
        {
            long seed1 = this.seed1;
            long seed2 = this.seed2;
            long seed3 = this.seed3;

            oldSeed1 = seed1;
            oldSeed2 = seed2;
            oldSeed3 = seed3;

            long newSeed1 = (seed1 << 12) ^ (seed1 >> 19) ^ ((seed1 >> 6) ^ (seed1 << 12)) & 0x1FFF;
            long newSeed2 = 16 * seed2 ^ (seed2 >> 25) ^ ((16 * seed2) ^ (seed2 >> 23)) & 0x7F;
            long newSeed3 = (seed3 >> 11) ^ (seed3 << 17) ^ ((seed3 >> 8) ^ (seed3 << 17)) & 0x1FFFFF;

            this.seed1 = newSeed1;
            this.seed2 = newSeed2;
            this.seed3 = newSeed3;

            return (newSeed1 ^ newSeed2 ^ newSeed3) & 0xffffffffL;
        }

        public void Seed(int s1, int s2, int s3)
        {
            seed1 = s1 | 0x100000;
            oldSeed1 = s1 | 0x100000;

            seed2 = s2 | 0x1000;
            oldSeed2 = s2 | 0x1000;

            seed3 = s3 | 0x10;
            oldSeed3 = s3 | 0x10;
        }
    }
}
