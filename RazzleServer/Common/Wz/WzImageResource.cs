using System;

namespace RazzleServer.Common.Wz
{
    public class WzImageResource : IDisposable
    {
        private readonly bool parsed;
        private readonly WzImage img;

        public WzImageResource(WzImage img)
        {
            this.img = img;
            parsed = img.Parsed;
            if (!parsed)
            {
                img.ParseImage();
            }
        }

        public void Dispose()
        {
            if (!parsed)
            {
                img.UnparseImage();
            }
        }
    }
}
