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
        private readonly ILogger _log = LogManager.Log;

        public byte Id { get; }
        public string Label { get; }
        public int DestinationMapId { get; }
        public string DestinationLabel { get; }
        public string Script { get; }
        public bool IsOnlyOnce { get; }
        public int PortalType { get; }

        public bool IsSpawnPoint => Label == "sp";

        [JsonIgnore]
        public MapReference DestinationMap => DataProvider.Maps.Data[DestinationMapId];

        [JsonIgnore]
        public Portal Link => DataProvider.Maps.Data[DestinationMapId].Portals.FirstOrDefault(x => x.Label == DestinationLabel);

        public Portal() { }

        public Portal(WzImageProperty datum)
        {
            Id = byte.Parse(datum.Name);
            Position = new Point(datum["x"].GetShort(), datum["y"].GetShort());
            Label = datum["pn"].GetString();
            DestinationMapId = datum["tm"].GetInt();
            DestinationLabel = datum["tn"]?.GetString();
            Script = datum["script"]?.GetString();
            PortalType = datum["pt"].GetInt();
            IsOnlyOnce = (datum["onlyOnce"]?.GetInt() ?? 0) > 0;
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