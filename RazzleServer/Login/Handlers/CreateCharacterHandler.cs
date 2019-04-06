using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CreateCharacter)]
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
            var strength = packet.ReadByte();
            var dexterity = packet.ReadByte();
            var intelligence = packet.ReadByte();
            var luck = packet.ReadByte();
            var error = ValidateCharacterCreation(client.Server, client.World, name, face, hair, hairColor, skin, topId,
                bottomId, shoesId, weaponId, client.Account.Gender);

            var character = new Character
            {
                AccountId = client.Account.Id,
                WorldId = client.World,
                Name = name,
                Map = new Map(ServerConfig.Instance.DefaultMapId)
            };
            character.PrimaryStats = new CharacterStats(character)
            {
                Gender = client.Account.Gender,
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
            character.Create();

            using (var pw = new PacketWriter(ServerOperationCode.CreateCharacterResult))
            {
                pw.WriteBool(error);
                if (!error)
                {
                    pw.WriteBytes(character.ToByteArray());
                }

                client.Send(pw);
            }
        }

        private bool ValidateCharacterCreation(LoginServer server, byte world, string name, int face, int hair,
            int hairColor, byte skin, int topId, int bottomId, int shoesId, int weaponId, Gender gender)
        {
            var error = name.Length < 4
                        || name.Length > 12
                        || server.CharacterExists(name, world)
                        || DataProvider.CreationData.ForbiddenNames.Any(name.Contains);

            switch (gender)
            {
                case Gender.Male:
                    error |= DataProvider.CreationData.MaleSkins.All(x => x != skin)
                             || DataProvider.CreationData.MaleFaces.All(x => x != face)
                             || DataProvider.CreationData.MaleHairs.All(x => x != hair)
                             || DataProvider.CreationData.MaleHairColors.All(x => x != hairColor)
                             || DataProvider.CreationData.MaleTops.All(x => x != topId)
                             || DataProvider.CreationData.MaleBottoms.All(x => x != bottomId)
                             || DataProvider.CreationData.MaleShoes.All(x => x != shoesId)
                             || DataProvider.CreationData.MaleWeapons.All(x => x != weaponId);
                    break;
                case Gender.Female:
                    error |= DataProvider.CreationData.FemaleSkins.All(x => x != skin)
                             || DataProvider.CreationData.FemaleFaces.All(x => x != face)
                             || DataProvider.CreationData.FemaleHairs.All(x => x != hair)
                             || DataProvider.CreationData.FemaleHairColors.All(x => x != hairColor)
                             || DataProvider.CreationData.FemaleTops.All(x => x != topId)
                             || DataProvider.CreationData.FemaleBottoms.All(x => x != bottomId)
                             || DataProvider.CreationData.FemaleShoes.All(x => x != shoesId)
                             || DataProvider.CreationData.FemaleWeapons.All(x => x != weaponId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }

            return error;
        }
    }
}
