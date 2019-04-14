using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Wz;

namespace RazzleServer.DataProvider.References
{
    public class PortalReference
    {
        public byte Id { get; set; }
        public string Label { get; set; }
        public int DestinationMapId { get; set; }
        public string DestinationLabel { get; set; }
        public PortalType Type { get; set; }
        public Point Position { get; set; }

        public PortalReference()
        {
        }

        public PortalReference(WzImageProperty img)
        {
            Id = byte.Parse(img.Name);
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Label = img["pn"].GetString();
            DestinationMapId = img["tm"].GetInt();
            DestinationLabel = img["tn"]?.GetString();
            Type = (PortalType)img["pt"].GetInt();
        }
    }
}
