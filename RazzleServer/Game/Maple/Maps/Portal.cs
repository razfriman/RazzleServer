using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : MapObject
    {
        private readonly ILogger Log = LogManager.Log;

        public byte ID { get; private set; }
        public string Label { get; private set; }
        public int DestinationMapID { get; private set; }
        public string DestinationLabel { get; private set; }
        public string Script { get; private set; }

        public bool IsSpawnPoint => Label == "sp";

        public Map DestinationMap => DataProvider.Maps[DestinationMapID];

        public Portal Link => DataProvider.Maps[DestinationMapID].Portals[DestinationLabel];

        public Portal(Datum datum)
        {
            ID = (byte)(int)datum["id"];
            Label = (string)datum["label"];
            Position = new Point((short)datum["x_pos"], (short)datum["y_pos"]);
            DestinationMapID = (int)datum["destination"];
            DestinationLabel = (string)datum["destination_label"];
            Script = (string)datum["script"];
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