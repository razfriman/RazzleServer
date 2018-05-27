using System.Collections.Generic;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data.References
{
    public class ReactorReference
    {
        public int MapleId { get; private set; }
        public string Label { get; private set; }
        public byte State { get; set; }
        public SpawnPoint SpawnPoint { get; private set; }
        public List<ReactorState> States { get; set; } = new List<ReactorState>();

        public ReactorReference()
        {

        }

        public ReactorReference(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleId = id;
            Label = img["action"]?.GetString();

            foreach (var state in img.WzProperties)
            {
                if (int.TryParse(state.Name, out var stateNumber))
                {
                    States.Add(new ReactorState(state));
                }
            }
        }
    }
}
