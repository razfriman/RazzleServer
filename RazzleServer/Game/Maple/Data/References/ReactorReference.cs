using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class ReactorReference
    {
        public int MapleId { get; set; }
        public Rectangle? Bounds { get; set; }
        public Dictionary<sbyte, ReactorStateReference> States { get; set; } = new Dictionary<sbyte, ReactorStateReference>();
        public string Script { get; set; }

        public ReactorReference() { }

        public ReactorReference(WzImage img, ReactorReference linkedStats = null)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
            Script = img["action"]?.GetString();

            if (linkedStats != null)
            {
                Bounds = linkedStats.Bounds;
                States = linkedStats.States;
                return;
            }

            var infoData = img["0"]?["event"]?["0"];
            if (infoData != null)
            {
                var areaSet = false;
                var i = (sbyte)0;

                while (infoData != null)
                {
                    var type = (ReactorEventType)(infoData["type"]?.GetInt() ?? 0);
                    var nextState = (sbyte)(infoData["state"]?.GetInt() ?? 0);
                    var itemId = 0;
                    var itemCount = 0;

                    if (type == ReactorEventType.HitByItem)
                    {
                        itemId = infoData["0"]?.GetInt() ?? 0;
                        itemCount = infoData["1"]?.GetInt() ?? 0;

                        if (!areaSet)
                        {
                            Bounds = new Rectangle(infoData["lt"].GetPoint(), infoData["rb"].GetPoint());
                        }
                    }

                    States[i] = new ReactorStateReference
                    {
                        State = i,
                        NextState = nextState,
                        Type = type,
                        ItemId = itemId,
                        ItemCount = itemCount
                    };

                    i++;
                    infoData = img[i.ToString()]?["event"]?["0"];
                }
            }
            else
            {
                States[0] = new ReactorStateReference
                {
                    State = 0,
                    Type = ReactorEventType.Dummy,
                    NextState = 0
                };
            }
        }
    }
}
