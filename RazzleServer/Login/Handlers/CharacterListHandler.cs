using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldSelect)]
    public class CharacterListHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            client.World = packet.ReadByte();
            client.Channel = packet.ReadByte();

            var characters = client.Server.GetCharacters(client.World, client.Account.ID);
            /*
             outPacket.WriteInt(accountID);

                foreach (Datum datum in new Datums("characters").PopulateWith("ID", "AccountID = {0} AND WorldID = {1}", accountID, WvsGame.WorldID))
                {
                    Character character = new Character((int)datum["ID"]);
                    character.Load();

                    byte[] entry = character.ToByteArray();

                    outPacket.WriteByte((byte)entry.Length);
                    outPacket.WriteBytes(entry);
                }
             */

            using (var oPacket = new PacketWriter(ServerOperationCode.SelectWorldResult))
            {
                oPacket.WriteBool(false);
                oPacket.WriteByte((byte)characters.Count);
                characters.ForEach(x => oPacket.WriteBytes(x));
                oPacket.WriteInt(client.Account.MaxCharacters);
                client.Send(oPacket);
            }
        }
    }
}