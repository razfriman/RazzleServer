using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Maps
{
    public class Portal : MapObject
    {
        public byte ID { get; private set; }
        public string Label { get; private set; }
        public int DestinationMapID { get; private set; }
        public string DestinationLabel { get; private set; }
        public string Script { get; private set; }

        public bool IsSpawnPoint
        {
            get
            {
                return this.Label == "sp";
            }
        }

        public Map DestinationMap
        {
            get
            {
                return DataProvider.Maps[this.DestinationMapID];
            }
        }

        public Portal Link
        {
            get
            {
                return DataProvider.Maps[this.DestinationMapID].Portals[this.DestinationLabel];
            }
        }

        public Portal(Datum datum)
        {
            this.ID = (byte)(int)datum["id"];
            this.Label = (string)datum["label"];
            this.Position = new Point((short)datum["x_pos"], (short)datum["y_pos"]);
            this.DestinationMapID = (int)datum["destination"];
            this.DestinationLabel = (string)datum["destination_label"];
            this.Script = (string)datum["script"];
        }

        public virtual void Enter(Character character)
        {
            Log.Warn("'{0}' attempted to enter an unimplemented portal '{1}'.", character.Name, this.Script);

            using (PacketReader oPacket = new Packet(ServerOperationCode.TransferFieldReqInogred))
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