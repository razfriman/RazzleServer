using RazzleServer.Common.Util;
using RazzleServer.Wz;

namespace RazzleServer.DataProvider.References
{
    public class SeatReference
    {
        public short Id { get; }
        public Point Position { get; set; }
        
        public SeatReference()
        {
        }

        public SeatReference(WzImageProperty img)
        {
            Id = short.Parse(img.Name);
            Position = img.GetPoint();
        }
    }
}
