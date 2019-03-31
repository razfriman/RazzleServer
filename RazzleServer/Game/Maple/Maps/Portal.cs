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
        public bool IsOnlyOnce { get; set; }
        public int PortalType { get; set; }
        public bool IsOpen { get; set; } = true;

        public bool IsSpawnPoint => Label == "sp";

        [JsonIgnore] public MapReference DestinationMap => DataProvider.Maps.Data[DestinationMapId];

        [JsonIgnore]
        public Portal Link => DataProvider.Maps.Data[DestinationMapId].Portals
            .FirstOrDefault(x => x.Label == DestinationLabel);

        public Portal() { }

        public Portal(WzImageProperty img)
        {
            Id = byte.Parse(img.Name);
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Label = img["pn"].GetString();
            DestinationMapId = img["tm"].GetInt();
            DestinationLabel = img["tn"]?.GetString();
            PortalType = img["pt"].GetInt();
            IsOnlyOnce = (img["onlyOnce"]?.GetInt() ?? 0) > 0;
        }

        public void Enter(Character character)
        {
            if (!character.Map.Portals.ContainsPortal(Label))
            {
                character.LogCheatWarning(CheatType.InvalidMapChange);
                return;
            }

            if (!IsOpen)
            {
                SendMapTransferResult(character, MapTransferResult.PortalClosed);
                return;
            }

            character.ChangeMap(DestinationMapId, DestinationLabel);
        }

        public static void SendMapTransferResult(Character character, MapTransferResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.TransferFieldReqIgnored))
            {
                pw.WriteByte(result);
                character.Client.Send(pw);
            }
        }

        public static void PlaySoundEffect(Character character) => character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
    }
}
