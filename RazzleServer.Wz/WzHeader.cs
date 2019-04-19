namespace RazzleServer.Wz
{
    public class WzHeader
    {
        public string Ident { get; set; }

        public string Copyright { get; set; }

        public ulong FSize { get; set; }

        public uint FStart { get; set; }

        public static WzHeader GetDefault()
        {
            var header = new WzHeader
            {
                Ident = "PKG1", Copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS", FStart = 60, FSize = 0
            };
            return header;
        }
    }
}
