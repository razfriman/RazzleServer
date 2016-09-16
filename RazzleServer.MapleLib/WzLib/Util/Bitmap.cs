using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB.WzLib.Util
{
    public class Bitmap
    {
        private int mWidth;
        private int mHeight;
        private PixelFormat format32bppArgb;

        public Bitmap(int mWidth, int mHeight, PixelFormat format32bppArgb)
        {
            this.mWidth = mWidth;
            this.mHeight = mHeight;
            this.format32bppArgb = format32bppArgb;
        }

        public Bitmap(int mWidth, int mHeight)
        {
            this.mWidth = mWidth;
            this.mHeight = mHeight;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        internal void SetPixel(int x, int y, Color color)
        {
            throw new NotImplementedException();
        }

        internal Color GetPixel(int j, int i)
        {
            throw new NotImplementedException();
        }

        internal void UnlockBits(BitmapData bmpData)
        {
            throw new NotImplementedException();
        }

        internal BitmapData LockBits(Rectangle rectangle, object writeOnly, PixelFormat format16bppRgb565)
        {
            throw new NotImplementedException();
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
