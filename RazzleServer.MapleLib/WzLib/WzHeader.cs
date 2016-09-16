using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleLib.WzLib
{
    public class WzHeader
    {
        private string mIdent;
        private string mCopyright;
        private ulong mFSize;
        private uint mFStart;
        private uint mExtraBytes;

        public string Ident
        {
            get { return mIdent; }
            set { mIdent = value; }
        }

        public string Copyright
        {
            get { return mCopyright; }
            set { mCopyright = value; }
        }

        public ulong FSize
        {
            get { return mFSize; }
            set { mFSize = value; }
        }

        public uint FStart
        {
            get { return mFStart; }
            set { mFStart = value; }
        }

        public uint ExtraBytes
        {
            get { return mExtraBytes; }
            set { mExtraBytes = value; }
        }

        public void RecalculateFileStart()
        {
            mFStart = (uint)(mIdent.Length + sizeof(ulong) + sizeof(uint) + mCopyright.Length + 1) + mExtraBytes;
        }

        public static WzHeader GetDefault()
        {
            WzHeader header = new WzHeader();
            header.mIdent = "PKG1";
            header.mCopyright = "Package file v1.0 Copyright 2002 Wizet, ZMS";
            header.mFStart = 60;
            header.mFSize = 0;
            header.mExtraBytes = 0;
            return header;
        }
    }
}