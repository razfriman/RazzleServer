using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Inventory;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System.Collections.Generic;
using MapleLib.PacketLib;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.CREATE_CHAR)]
    public class CreateCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {

            MapleCharacter newCharacter = MapleCharacter.GetDefaultCharacter(client);
            string name = packet.ReadString();
            if (!Functions.IsAlphaNumerical(name))
                return;

            bool nameAvailable = !MapleCharacter.CharacterExists(name);
            if (nameAvailable)
            {
                newCharacter.Job = (short)packet.ReadInt();
                newCharacter.Face = packet.ReadInt();
                newCharacter.Hair = packet.ReadInt() + packet.ReadInt();
                newCharacter.Skin = (byte) packet.ReadInt();
                var top = packet.ReadInt();
                var bottom = packet.ReadInt();
                var shoes = packet.ReadInt();
                var weapon = packet.ReadInt();
                newCharacter.Gender = packet.ReadByte();
                newCharacter.Name = name;

                Dictionary<MapleEquipPosition, int> items = new Dictionary<MapleEquipPosition, int>();
                items.Add(MapleEquipPosition.Top, top);
                items.Add(MapleEquipPosition.Bottom, bottom);
                items.Add(MapleEquipPosition.Shoes, shoes);
                items.Add(MapleEquipPosition.Weapon, weapon);


                foreach (KeyValuePair<MapleEquipPosition, int> item in items)
                {
                    //var wzInfo = DataBuffer.GetEquipById(item.Value);
                    //if (wzInfo != null)
                    {
                        MapleEquip equip = new MapleEquip(item.Value, "Character creation", position: (short)item.Key);
                        //equip.SetDefaultStats(wzInfo);
                        newCharacter.Inventory.SetItem(equip, MapleInventoryType.Equipped, equip.Position, false);
                    }
                }
                newCharacter.SetKeyMap(GameConstants.DefaultBasicKeyBinds);
                newCharacter.SetQuickSlotKeys(GameConstants.DefaultBasicQuickSlotKeyMap);
                newCharacter.InsertCharacter();
                newCharacter?.Inventory.SaveToDatabase(true);
            }

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.ADD_NEW_CHAR_ENTRY);
            pw.WriteBool(!nameAvailable);
            if (nameAvailable)
            {
                MapleCharacter.AddCharEntry(pw, newCharacter);
            }
            client.Send(pw);

            newCharacter?.Release();
        }
    }
}