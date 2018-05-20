namespace RazzleServer.Common.WzLib
{
	public class WzHeader
	{
        private string ident;
        private string copyright;
        private ulong fsize;
        private uint fstart;

        public string Ident
        {
            get => ident;
	        set => ident = value;
        }

        public string Copyright
        {
            get => copyright;
	        set => copyright = value;
        }

        public ulong FSize
        {
            get => fsize;
	        set => fsize = value;
        }

		public uint FStart 
        {
            get => fstart;
			set => fstart = value;
		}

        public void RecalculateFileStart()
        {
            fstart = (uint)(ident.Length + sizeof(ulong) + sizeof(uint) + copyright.Length + 1);
        }

		public static WzHeader GetDefault()
		{
			var header = new WzHeader();
			header.ident = "PKG1";
			header.copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS";
			header.fstart = 60;
			header.fsize = 0;
			return header;
		}
	}
}