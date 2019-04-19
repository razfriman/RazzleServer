using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : IMapObject
    {
        public byte Id { get; set; }
        public string Label { get; set; }
        public int DestinationMapId { get; set; }
        public string DestinationLabel { get; set; }
        public PortalType Type { get; set; }
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
        public bool IsSpawnPoint => Label == "sp";
        public MapReference DestinationMap => CachedData.Maps.Data[DestinationMapId];

        public Portal(PortalReference reference)
        {
            Id = reference.Id;
            Position = reference.Position;
            Label = reference.Label;
            DestinationMapId = reference.DestinationMapId;
            DestinationLabel = reference.DestinationLabel;
            Type = reference.Type;
        }

        public void Enter(Character character)
        {
            if (!character.Map.Portals.ContainsPortal(Label))
            {
                character.LogCheatWarning(CheatType.InvalidMapChange);
                SendMapTransferResult(character, MapTransferResult.PortalClosed);
                return;
            }

            character.ChangeMap(DestinationMapId, DestinationLabel);
        }

        public static void SendMapTransferResult(Character character, MapTransferResult result)
        {
            using var pw = new PacketWriter(ServerOperationCode.TransferFieldReqIgnored);
            pw.WriteByte(result);
            character.Send(pw);
        }

        public static void PlaySoundEffect(Character character) =>
            character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
    }
}
