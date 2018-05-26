using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class ReactorState
    {
        public ReactorEventType Type { get; private set; }
        public byte State { get; private set; }
        public byte NextState { get; private set; }
        public int Timeout { get; private set; }
        public int ItemId { get; private set; }
        public short Quantity { get; private set; }
        public Rectangle Boundaries { get; private set; }

        public ReactorState(WzImageProperty img)
        {
            var eventImg = img?["event"]?["0"];

            if (eventImg != null)
            {
                Type = (ReactorEventType)eventImg["type"].GetInt();
                State = (byte)eventImg["state"].GetInt();

                if (eventImg["lt"] != null && eventImg["rb"] != null)
                {
                    Boundaries = new Rectangle(eventImg["lt"].GetPoint(), eventImg["rb"].GetPoint());
                }
            }

            //this.NextState = (byte)(sbyte)img["next_state"];
            //this.Timeout = (int)img["timeout"];
            //this.ItemId = (int)img["itemid"];
            //this.Quantity = (short)img["quantity"];
            //this.Boundaries = new Rectangle(new Point((short)img["ltx"], (short)img["lty"]), new Point((short)img["ltx"], (short)img["lty"]));
        }
    }
}
