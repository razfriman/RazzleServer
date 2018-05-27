using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterCreate)]
    public class CreateCharacterHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var face = packet.ReadInt();
            var hair = packet.ReadInt();
            var hairColor = packet.ReadInt();
            var skin = (byte)packet.ReadInt();
            var topId = packet.ReadInt();
            var bottomId = packet.ReadInt();
            var shoesId = packet.ReadInt();
            var weaponId = packet.ReadInt();
            var gender = (Gender)packet.ReadByte();
            var strength = packet.ReadByte();
            var dexterity = packet.ReadByte();
            var intelligence = packet.ReadByte();
            var luck = packet.ReadByte();
            var error = ValidateCharacterCreation(name, face, hair, hairColor, skin, topId, bottomId, shoesId, weaponId, gender);

            var character = new Character
            {
                AccountId = client.Account.Id,
                WorldId = client.World,
                Name = name,
                Gender = gender,
                Skin = skin,
                Face = face,
                Hair = hair + hairColor,
                Level = 1,
                Job = Job.Beginner,
                Strength = strength,
                Dexterity = dexterity,
                Intelligence = intelligence,
                Luck = luck,
                MaxHealth = 50,
                MaxMana = 5,
                Health = 50,
                Mana = 5
            };

            character.Items.Add(new Item(topId, equipped: true));
            character.Items.Add(new Item(bottomId, equipped: true));
            character.Items.Add(new Item(shoesId, equipped: true));
            character.Items.Add(new Item(weaponId, equipped: true));
            character.Items.Add(new Item(4161001), forceGetSlot: true);
            CreateDefaultKeymap(character);
            character.Create();

            client.Send(LoginPackets.CreateNewCharacterResult(error, character));
        }

        private static bool ValidateCharacterCreation(string name, int face, int hair, int hairColor, byte skin, int topId, int bottomId, int shoesId, int weaponId, Gender gender)
        {
            var error = name.Length < 4
                                         || name.Length > 12
                                         || Character.CharacterExists(name)
                                         || DataProvider.CreationData.ForbiddenNames.Any(name.Contains);

            if (gender == Gender.Male)
            {
                error |= DataProvider.CreationData.MaleSkins.All(x => x != skin)
                         || DataProvider.CreationData.MaleFaces.All(x => x != face)
                         || DataProvider.CreationData.MaleHairs.All(x => x != hair)
                         || DataProvider.CreationData.MaleHairColors.All(x => x != hairColor)
                         || DataProvider.CreationData.MaleTops.All(x => x != topId)
                         || DataProvider.CreationData.MaleBottoms.All(x => x != bottomId)
                         || DataProvider.CreationData.MaleShoes.All(x => x != shoesId)
                         || DataProvider.CreationData.MaleWeapons.All(x => x != weaponId);
            }
            else if (gender == Gender.Female)
            {
                error |= DataProvider.CreationData.FemaleSkins.All(x => x != skin)
                         || DataProvider.CreationData.FemaleFaces.All(x => x != face)
                         || DataProvider.CreationData.FemaleHairs.All(x => x != hair)
                         || DataProvider.CreationData.FemaleHairColors.All(x => x != hairColor)
                         || DataProvider.CreationData.FemaleTops.All(x => x != topId)
                         || DataProvider.CreationData.FemaleBottoms.All(x => x != bottomId)
                         || DataProvider.CreationData.FemaleShoes.All(x => x != shoesId)
                         || DataProvider.CreationData.FemaleWeapons.All(x => x != weaponId);
            }

            return error;
        }

        private static void CreateDefaultKeymap(Character character)
        {
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
        }
    }
}