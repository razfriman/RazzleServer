using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using MapleLib.WzLib.Util;
using RazzleServer.DB.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains the information for a bitmap
    /// </summary>
    public class WzPngProperty : AWzImageProperty
    {
        #region Fields
        internal int mWidth, mHeight, mFormat, mFormat2;
        internal byte[] mCompressedBytes;
        internal Bitmap mPNG;
        internal bool mIsNew = false;
        internal AWzObject mParent;
        internal WzImage mImgParent;
        internal WzBinaryReader mWzReader;
        internal long mOffsets;

        #endregion

        #region Inherited Members
        public override object WzValue
        {
            get { return GetPNG(); }
            set
            {
                if (value is Bitmap) SetPNG((Bitmap)value);
                else mCompressedBytes = (byte[])value;
            }
        }
        /// <summary>
        /// The parent of the object
        /// </summary>
        public override AWzObject Parent { get { return mParent; } internal set { mParent = value; } }
        /// <summary>
        /// The image that this property is contained in
        /// </summary>
        public override WzImage ParentImage { get { return mImgParent; } internal set { mImgParent = value; } }
        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name { get { return "PNG"; } set { } }
        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType { get { return WzPropertyType.PNG; } }
        public override void WriteValue(WzBinaryWriter pWriter)
        {
            throw new NotImplementedException("Cannot write a PngProperty");
        }
        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            mCompressedBytes = null;
            if (mPNG != null)
            {
                mPNG.Dispose();
                mPNG = null;
            }
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// The width of the bitmap
        /// </summary>
        public int Width { get { return mWidth; } set { mWidth = value; } }
        /// <summary>
        /// The height of the bitmap
        /// </summary>
        public int Height { get { return mHeight; } set { mHeight = value; } }
        /// <summary>
        /// The format of the bitmap
        /// </summary>
        public int Format { get { return mFormat + mFormat2; } set { mFormat = value; mFormat2 = 0; } }
        
        /// <summary>
        /// Creates a blank WzPngProperty
        /// </summary>
        public WzPngProperty() { }
        internal WzPngProperty(WzBinaryReader pReader)
        {
            // Read compressed bytes
            mWidth = pReader.ReadCompressedInt();
            mHeight = pReader.ReadCompressedInt();
            mFormat = pReader.ReadCompressedInt();
            mFormat2 = pReader.ReadByte();
            pReader.BaseStream.Position += 4;
            mOffsets = pReader.BaseStream.Position;
            int len = pReader.ReadInt32() - 1;
            pReader.BaseStream.Position += 1;

            if (len > 0)
                pReader.BaseStream.Position += len;
            mWzReader = pReader;
        }
        #endregion

        #region Parsing Methods
        public byte[] GetCompressedBytes(bool pSaveInMemory = false)
        {
            if (mCompressedBytes == null)
            {
                long pos = mWzReader.BaseStream.Position;
                mWzReader.BaseStream.Position = mOffsets;
                int len = mWzReader.ReadInt32() - 1;
                mWzReader.BaseStream.Position += 1;
                if (len > 0)
                    mCompressedBytes = mWzReader.ReadBytes(len);
                mWzReader.BaseStream.Position = pos;
                if (!pSaveInMemory)
                {
                    mCompressedBytes = null;
                    return mCompressedBytes;
                }
            }
            return mCompressedBytes;
        }

        public void SetPNG(Bitmap pPng)
        {
            this.mPNG = pPng;
            CompressPng(pPng);
        }

        public Bitmap GetPNG(bool pSaveInMemory = false)
        {
            if (mPNG == null)
            {
                long pos = mWzReader.BaseStream.Position;
                mWzReader.BaseStream.Position = mOffsets;
                int len = mWzReader.ReadInt32() - 1;
                mWzReader.BaseStream.Position += 1;
                if (len > 0)
                    mCompressedBytes = mWzReader.ReadBytes(len);
                ParsePng();
                mWzReader.BaseStream.Position = pos;
                if (!pSaveInMemory)
                {
                    Bitmap pngImage = mPNG;
                    mPNG = null;
                    mCompressedBytes = null;
                    return pngImage;
                }
            }
            return mPNG;
        }

        internal byte[] Decompress(byte[] pCompressedBuffer, int pDecompressedSize)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(pCompressedBuffer, 2, pCompressedBuffer.Length - 2);
            byte[] buffer = new byte[pDecompressedSize];
            memStream.Position = 0;
            DeflateStream zip = new DeflateStream(memStream, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);
            zip.Dispose();
            memStream.Dispose();
            return buffer;
        }
        internal byte[] Compress(byte[] pDecompressedBuffer)
        {
            MemoryStream memStream = new MemoryStream();
            DeflateStream zip = new DeflateStream(memStream, CompressionMode.Compress, true);
            zip.Write(pDecompressedBuffer, 0, pDecompressedBuffer.Length);
            memStream.Position = 0;
            byte[] buffer = new byte[memStream.Length + 2];
            Console.WriteLine(BitConverter.ToString(memStream.ToArray()));
            memStream.Read(buffer, 2, buffer.Length - 2);
            memStream.Dispose();
            zip.Dispose();
            System.Buffer.BlockCopy(new byte[] { 0x78, 0x9C }, 0, buffer, 0, 2);
            return buffer;
        }
        internal void ParsePng()
        {
            DeflateStream zlib;
            int uncompressedSize = 0;
            int x = 0, y = 0, b = 0, g = 0;
            Bitmap bmp = null;
            byte[] decBuf;

            BinaryReader reader = new BinaryReader(new MemoryStream(mCompressedBytes));
            ushort header = reader.ReadUInt16();
            if (header == 0x9C78)
            {
                zlib = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
            }
            else
            {
                reader.BaseStream.Position -= 2;
                MemoryStream dataStream = new MemoryStream();
                int blocksize = 0;
                int endOfPng = mCompressedBytes.Length;

                while (reader.BaseStream.Position < endOfPng)
                {
                    blocksize = reader.ReadInt32();
                    for (int i = 0; i < blocksize; i++)
                    {
                        dataStream.WriteByte((byte)(reader.ReadByte() ^ mImgParent.mReader.WzKey[i]));
                    }
                }
                dataStream.Position = 2;
                zlib = new DeflateStream(dataStream, CompressionMode.Decompress);
            }

            switch (mFormat + mFormat2)
            {
                case 1:
                    bmp = new Bitmap(mWidth, mHeight, PixelFormat.Format32bppArgb);
                    uncompressedSize = mWidth * mHeight * 2;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    byte[] argb = new Byte[uncompressedSize * 2];
                    for (int i = 0; i < uncompressedSize; i++)
                    {
                        b = decBuf[i] & 0x0F; b |= (b << 4); argb[i * 2] = (byte)b;
                        g = decBuf[i] & 0xF0; g |= (g >> 4); argb[i * 2 + 1] = (byte)g;
                    }
                    break;
                case 2:
                    bmp = new Bitmap(mWidth, mHeight, PixelFormat.Format32bppArgb);
                    uncompressedSize = mWidth * mHeight * 4;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    break;
                case 513:
                    bmp = new Bitmap(mWidth, mHeight, PixelFormat.Format16bppRgb565);
                    uncompressedSize = mWidth * mHeight * 2;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    break;

                case 517:
                    bmp = new Bitmap(mWidth, mHeight);
                    uncompressedSize = mWidth * mHeight / 128;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    byte iB = 0;
                    for (int i = 0; i < uncompressedSize; i++)
                    {
                        for (byte j = 0; j < 8; j++)
                        {
                            iB = Convert.ToByte(((decBuf[i] & (0x01 << (7 - j))) >> (7 - j)) * 0xFF);
                            for (int k = 0; k < 16; k++)
                            {
                                if (x == mWidth) { x = 0; y++; }
                                bmp.SetPixel(x, y, Color.FromArgb(0xFF, iB, iB, iB));
                                x++;
                            }
                        }
                    }
                    break;
            }
            mPNG = bmp;
        }

        internal void CompressPng(Bitmap pBmp)
        {
            byte[] buf = new byte[pBmp.Width * pBmp.Height * 8];
            mFormat = 2;
            mFormat2 = 0;
            mWidth = pBmp.Width;
            mHeight = pBmp.Height;

            int curPos = 0;
            for (int i = 0; i < mHeight; i++)
                for (int j = 0; j < mWidth; j++)
                {
                    Color curPixel = pBmp.GetPixel(j, i);
                    buf[curPos] = curPixel.B;
                    buf[curPos + 1] = curPixel.G;
                    buf[curPos + 2] = curPixel.R;
                    buf[curPos + 3] = curPixel.A;
                    curPos += 4;
                }
            mCompressedBytes = Compress(buf);
            if (mIsNew)
            {
                MemoryStream memStream = new MemoryStream();
                WzBinaryWriter writer = new WzBinaryWriter(memStream, WzTool.GetIvByMapleVersion(WzMapleVersion.GMS));
                writer.Write(2);
                for (int i = 0; i < 2; i++)
                {
                    writer.Write((byte)(mCompressedBytes[i] ^ writer.WzKey[i]));
                }
                writer.Write(mCompressedBytes.Length - 2);
                for (int i = 2; i < mCompressedBytes.Length; i++)
                    writer.Write((byte)(mCompressedBytes[i] ^ writer.WzKey[i - 2]));

                // TODO - GET THE BUFFER
                //mCompressedBytes = memStream.GetBuffer();
            }
        }
        #endregion

        #region Cast Values
        public override WzPngProperty ToPngProperty(WzPngProperty pDef)
        {
            return this;
        }

        public override Bitmap ToBitmap(Bitmap pDef)
        {
            return GetPNG(false);
        }

        public override byte[] ToBytes(byte[] pDef)
        {
            return base.ToBytes(pDef);
        }
        #endregion

    }
}