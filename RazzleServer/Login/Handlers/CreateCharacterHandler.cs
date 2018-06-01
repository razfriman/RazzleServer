using System.Linq;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Maps;

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
            var error = ValidateCharacterCreation(client.Server, client.World, name, face, hair, hairColor, skin, topId, bottomId, shoesId, weaponId, gender);

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
                Mana = 5,
                Map = new Map(ServerConfig.Instance.DefaultMapId)
            };

            character.Items.Add(new Item(topId, equipped: true));
            character.Items.Add(new Item(bottomId, equipped: true));
            character.Items.Add(new Item(shoesId, equipped: true));
            character.Items.Add(new Item(weaponId, equipped: true));
            character.Items.Add(new Item(4161001), forceGetSlot: true);
            character.Keymap.CreateDefaultKeymap();
            character.Create();

            using (var pw = new PacketWriter(ServerOperationCode.CreateNewCharacterResult))
            {
                pw.WriteBool(error);
                pw.WriteBytes(character.ToByteArray());
                client.Send(pw);
            }
        }

        private bool ValidateCharacterCreation(LoginServer server, byte world, string name, int face, int hair, int hairColor, byte skin, int topId, int bottomId, int shoesId, int weaponId, Gender gender)
        {
            var error = name.Length < 4
                            || name.Length > 12
                            || server.CharacterExists(name, world)
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
    }
}