using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CREATE_CHAR)]
    public class CreateCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {

            MapleCharacter newCharacter = MapleCharacter.GetDefaultCharacter(client);
            string name = packet.ReadMapleString();
            if (!Functions.IsAlphaNumerical(name))
                return;

            bool nameAvailable = !MapleCharacter.CharacterExists(name);
            if (nameAvailable)
            {
                newCharacter.Job = (short)packet.ReadInt();
                newCharacter.Face = packet.ReadInt();
                newCharacter.Hair = packet.ReadInt() + packet.ReadInt();

                var top = packet.ReadInt();
                var bottom = packet.ReadInt();
                var shoes = packet.ReadInt();
                var weapon = packet.ReadInt();

                newCharacter.Gender = packet.ReadByte();
                newCharacter.Name = name;
                newCharacter.MapID = 1000000;

                newCharacter.InsertCharacter();
                //newCharacter.Inventory.SaveToDatabase(true);
            }

            PacketWriter pw = new PacketWriter(SMSGHeader.ADD_NEW_CHAR_ENTRY);
            pw.WriteBool(!nameAvailable);
            if (nameAvailable)
            {
                MapleCharacter.AddCharEntry(pw, newCharacter);
            }
            client.SendPacket(pw);

            newCharacter?.Release();
        }
    }
}