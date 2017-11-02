using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CREATE_CHAR)]
    public class CreateCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            int accountID = packet.ReadInt();
            string name = packet.ReadString();
            int face = packet.ReadInt();
            int hair = packet.ReadInt();
            int hairColor = packet.ReadInt();
            byte skin = (byte)packet.ReadInt();
            int topID = packet.ReadInt();
            int bottomID = packet.ReadInt();
            int shoesID = packet.ReadInt();
            int weaponID = packet.ReadInt();
            Gender gender = (Gender)packet.ReadByte();

            bool error = name.Length < 4 || name.Length > 12
                || Character.CharacterExists(name)
                             || DataProvider.CreationData.ForbiddenNames.Any(forbiddenWord => name.ToLowerInvariant().Contains(forbiddenWord));

            if (gender == Gender.Male)
            {
                error |= (!DataProvider.CreationData.MaleSkins.Any(x => x == skin)
                    || !DataProvider.CreationData.MaleFaces.Any(x => x == face)
                    || !DataProvider.CreationData.MaleHairs.Any(x => x == hair)
                    || !DataProvider.CreationData.MaleHairColors.Any(x => x == hairColor)
                    || !DataProvider.CreationData.MaleTops.Any(x => x == topID)
                    || !DataProvider.CreationData.MaleBottoms.Any(x => x == bottomID)
                    || !DataProvider.CreationData.MaleShoes.Any(x => x == shoesID)
                    || !DataProvider.CreationData.MaleWeapons.Any(x => x == weaponID));
            }
            else if (gender == Gender.Female)
            {
                error |= (!DataProvider.CreationData.FemaleSkins.Any(x => x == skin)
                    || !DataProvider.CreationData.FemaleFaces.Any(x => x == face)
                    || !DataProvider.CreationData.FemaleHairs.Any(x => x == hair)
                    || !DataProvider.CreationData.FemaleHairColors.Any(x => x == hairColor)
                    || !DataProvider.CreationData.FemaleTops.Any(x => x == topID)
                    || !DataProvider.CreationData.FemaleBottoms.Any(x => x == bottomID)
                    || !DataProvider.CreationData.FemaleShoes.Any(x => x == shoesID)
                    || !DataProvider.CreationData.FemaleWeapons.Any(x => x == weaponID));
            }
            else // NOTE: Not allowed to choose "both" genders at character creation.
            {
                error = true;
            }

            Character character = new Character
            {
                AccountID = accountID,
                WorldID = client.World,
                Name = name,
                Gender = gender,
                Skin = skin,
                Face = face,
                Hair = hair + hairColor,
                Level = 1,
                Job = Job.Beginner,
                Strength = 12,
                Dexterity = 5,
                Intelligence = 4,
                Luck = 4,
                MaxHealth = 50,
                MaxMana = 5,
                Health = 50,
                Mana = 5,
                AbilityPoints = 0,
                SkillPoints = 0,
                Experience = 0,
                Fame = 0,
                Map = DataProvider.Maps[10000],
                SpawnPoint = 0,
                Meso = 0
            };
            character.Items.Add(new Item(topID, equipped: true));
            character.Items.Add(new Item(bottomID, equipped: true));
            character.Items.Add(new Item(shoesID, equipped: true));
            character.Items.Add(new Item(weaponID, equipped: true));
            character.Items.Add(new Item(4161001), forceGetSlot: true);

            character.Keymap.Add(new Shortcut(KeymapKey.One, KeymapAction.AllChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Two, KeymapAction.PartyChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Three, KeymapAction.BuddyChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Four, KeymapAction.GuildChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Six, KeymapAction.SpouseChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Q, KeymapAction.QuestMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.W, KeymapAction.WorldMap));
            character.Keymap.Add(new Shortcut(KeymapKey.E, KeymapAction.EquipmentMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.R, KeymapAction.BuddyList));
            character.Keymap.Add(new Shortcut(KeymapKey.I, KeymapAction.ItemMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.O, KeymapAction.PartySearch));
            character.Keymap.Add(new Shortcut(KeymapKey.P, KeymapAction.PartyList));
            character.Keymap.Add(new Shortcut(KeymapKey.BracketLeft, KeymapAction.Shortcut));
            character.Keymap.Add(new Shortcut(KeymapKey.BracketRight, KeymapAction.QuickSlot));
            character.Keymap.Add(new Shortcut(KeymapKey.LeftCtrl, KeymapAction.Attack));
            character.Keymap.Add(new Shortcut(KeymapKey.S, KeymapAction.AbilityMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.G, KeymapAction.GuildList));
            character.Keymap.Add(new Shortcut(KeymapKey.H, KeymapAction.WhisperChat));
            character.Keymap.Add(new Shortcut(KeymapKey.K, KeymapAction.SkillMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.L, KeymapAction.QuestHelper));
            character.Keymap.Add(new Shortcut(KeymapKey.Semicolon, KeymapAction.Medal));
            character.Keymap.Add(new Shortcut(KeymapKey.Quote, KeymapAction.ExpandChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Backslash, KeymapAction.SetKey));
            character.Keymap.Add(new Shortcut(KeymapKey.Z, KeymapAction.PickUp));
            character.Keymap.Add(new Shortcut(KeymapKey.X, KeymapAction.Sit));
            character.Keymap.Add(new Shortcut(KeymapKey.C, KeymapAction.Messenger));
            character.Keymap.Add(new Shortcut(KeymapKey.M, KeymapAction.MiniMap));
            character.Keymap.Add(new Shortcut(KeymapKey.LeftAlt, KeymapAction.Jump));
            character.Keymap.Add(new Shortcut(KeymapKey.F1, KeymapAction.Cockeyed));
            character.Keymap.Add(new Shortcut(KeymapKey.F2, KeymapAction.Happy));
            character.Keymap.Add(new Shortcut(KeymapKey.F3, KeymapAction.Sarcastic));
            character.Keymap.Add(new Shortcut(KeymapKey.F4, KeymapAction.Crying));
            character.Keymap.Add(new Shortcut(KeymapKey.F5, KeymapAction.Outraged));
            character.Keymap.Add(new Shortcut(KeymapKey.F6, KeymapAction.Shocked));
            character.Keymap.Add(new Shortcut(KeymapKey.F7, KeymapAction.Annoyed));

            if (!error)
            {
                using (var outPacket = new PacketWriter(ServerOperationCode.CreateNewCharacterResult))
                {
                    outPacket.WriteInt(accountID);
                    outPacket.WriteBytes(character.ToByteArray());
                    client.Send(outPacket);
                }
            }
        }
    }
}