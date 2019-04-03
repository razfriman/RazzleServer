using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Net.Packet;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : MapObject
    {
        public byte Id { get; set; }
        public string Label { get; set; }
        public int DestinationMapId { get; set; }
        public string DestinationLabel { get; set; }
        public PortalType Type { get; set; }

        public bool IsSpawnPoint => Label == "sp";

        [JsonIgnore] public MapReference DestinationMap => DataProvider.Maps.Data[DestinationMapId];

        [JsonIgnore]
        public Portal Link => DataProvider.Maps.Data[DestinationMapId]?.Portals
            ?.FirstOrDefault(x => x.Label == DestinationLabel);

        public Portal() { }

        public Portal(WzImageProperty img)
        {
            Id = byte.Parse(img.Name);
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Label = img["pn"].GetString();
            DestinationMapId = img["tm"].GetInt();
            DestinationLabel = img["tn"]?.GetString();
            Type = (PortalType)img["pt"].GetInt();
        }

        public void Enter(Character character)
        {
            using (var pw = new PacketWriter(ServerOperationCode.TransferFieldReqIgnored))
            {
                pw.WriteByte(MapTransferResult.NoReason);
                character.Client.Send(pw);
            }
        }

        public void PlaySoundEffect(Character character) => character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
    }
}
