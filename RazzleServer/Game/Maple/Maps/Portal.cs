using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : MapObject
    {
        private readonly ILogger Log = LogManager.Log;

        public byte Id { get; private set; }
        public string Label { get; private set; }
        public int DestinationMapId { get; private set; }
        public string DestinationLabel { get; private set; }
        public string Script { get; private set; }
        public bool IsOnlyOnce { get; private set; }
        public int PortalType { get; private set; }

        public bool IsSpawnPoint => Label == "sp";

        [JsonIgnore]
        public Map DestinationMap => Map.Server[DestinationMapId];

        [JsonIgnore]
        public Portal Link => DataProvider.Maps.Data[DestinationMapId].Portals[DestinationLabel];

        public Portal() : base() { }

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
            Log.LogWarning($"'{character.Name}' attempted to enter an unimplemented portal '{Script}'");

            using (var oPacket = new PacketWriter(ServerOperationCode.TransferFieldReqInogred))
            {
                oPacket.WriteByte((byte)MapTransferResult.NoReason);
                character.Client.Send(oPacket);
            }
        }

        public void PlaySoundEffect(Character character)
        {
            character.ShowLocalUserEffect(UserEffect.PlayPortalSE);
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