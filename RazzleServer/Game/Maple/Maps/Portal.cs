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

        public void Enter(GameCharacter gameCharacter)
        {
            if (!gameCharacter.Map.Portals.ContainsPortal(Label))
            {
                gameCharacter.LogCheatWarning(CheatType.InvalidMapChange);
                SendMapTransferResult(gameCharacter, MapTransferResult.PortalClosed);
                return;
            }

            gameCharacter.ChangeMap(DestinationMapId, DestinationLabel);
        }

        public static void SendMapTransferResult(GameCharacter gameCharacter, MapTransferResult result)
        {
            using var pw = new PacketWriter(ServerOperationCode.TransferFieldReqIgnored);
            pw.WriteByte(result);
            gameCharacter.Send(pw);
        }

        public static void PlaySoundEffect(GameCharacter gameCharacter) => gameCharacter.ShowLocalUserEffect(UserEffect.PlayPortalSe);
    }
}
