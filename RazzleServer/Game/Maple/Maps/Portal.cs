using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : MapObject
    {
        private readonly ILogger _log = LogManager.CreateLogger<Portal>();

        public byte Id { get; set; }
        public string Label { get; set; }
        public int DestinationMapId { get; set; }
        public string DestinationLabel { get; set; }
        public string Script { get; set; }
        public bool IsOnlyOnce { get; set; }
        public int PortalType { get; set; }

        public bool IsSpawnPoint => Label == "sp";

        [JsonIgnore]
        public MapReference DestinationMap => DataProvider.Maps.Data[DestinationMapId];

        [JsonIgnore]
        public Portal Link => DataProvider.Maps.Data[DestinationMapId].Portals.FirstOrDefault(x => x.Label == DestinationLabel);

        public Portal() { }

        public Portal(WzImageProperty img)
        {
            Id = byte.Parse(img.Name);
            Position = new Point(img["x"].GetShort(), img["y"].GetShort());
            Label = img["pn"].GetString();
            DestinationMapId = img["tm"].GetInt();
            DestinationLabel = img["tn"]?.GetString();
            Script = img["script"]?.GetString();
            PortalType = img["pt"].GetInt();
            IsOnlyOnce = (img["onlyOnce"]?.GetInt() ?? 0) > 0;
        }

        public virtual void Enter(Character character)
        {
            _log.LogWarning($"'{character.Name}' attempted to enter an unimplemented portal '{Script}'");

            using (var oPacket = new PacketWriter(ServerOperationCode.TransferFieldReqInogred))
            {
                oPacket.WriteByte((byte)MapTransferResult.NoReason);
                character.Client.Send(oPacket);
            }
        }

        public void PlaySoundEffect(Character character)
        {
            character.ShowLocalUserEffect(UserEffect.PlayPortalSe);
        }

        public void ShowBalloonMessage(Character character, string text, short width, short height)
        {
            var oPacket = new PacketWriter(ServerOperationCode.BalloonMsg);
            oPacket.WriteString(text);
            oPacket.WriteShort(width);
            oPacket.WriteShort(height);
            oPacket.WriteByte(1);
            character.Client.Send(oPacket);
        }


        public void ShowTutorialMessage(Character character, string dataPath)
        {
            var oPacket = new PacketWriter(ServerOperationCode.Effect);
            oPacket.WriteByte((byte)UserEffect.AvatarOriented);
            oPacket.WriteString(dataPath);
            oPacket.WriteInt(1);
            character.Client.Send(oPacket);
        }
    }
}
