namespace RazzleServer.Player
{
    public class BuffStat
    {
        public int BitIndex { get; private set; }
        public bool IsStackingBuff { get; private set; }
        public bool UsesStacksAsValue { get; private set; }

        public BuffStat(int bitIndex, bool usesStacksAsValue = false, bool stackingBuff = false)
        {
            BitIndex = bitIndex;
            IsStackingBuff = stackingBuff;
            UsesStacksAsValue = usesStacksAsValue;
        }
    }
}
