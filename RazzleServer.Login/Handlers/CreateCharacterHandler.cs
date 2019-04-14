using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Login.Maple;
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

            var character = new LoginCharacter
            {
                AccountId = client.Account.Id,
                WorldId = client.World,
                Name = name,
                MapId = ServerConfig.Instance.DefaultMapId
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

            using var pw = new PacketWriter(ServerOperationCode.CreateCharacterResult);
            pw.WriteBool(error);
            if (!error)
            {
                pw.WriteBytes(character.ToByteArray());
            }

            client.Send(pw);
        }

        private bool ValidateCharacterCreation(LoginServer server, byte world, string name, int face, int hair,
            int hairColor, byte skin, int topId, int bottomId, int shoesId, int weaponId, Gender gender)
        {
            var error = name.Length < 4
                        || name.Length > 12
                        || server.CharacterExists(name, world)
                        || CachedData.CreationData.ForbiddenNames.Any(x =>
                            x.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            switch (gender)
            {
                case Gender.Male:
                    error |= CachedData.CreationData.MaleSkins.All(x => x != skin)
                             || CachedData.CreationData.MaleFaces.All(x => x != face)
                             || CachedData.CreationData.MaleHairs.All(x => x != hair)
                             || CachedData.CreationData.MaleHairColors.All(x => x != hairColor)
                             || CachedData.CreationData.MaleTops.All(x => x != topId)
                             || CachedData.CreationData.MaleBottoms.All(x => x != bottomId)
                             || CachedData.CreationData.MaleShoes.All(x => x != shoesId)
                             || CachedData.CreationData.MaleWeapons.All(x => x != weaponId);
                    break;
                case Gender.Female:
                    error |= CachedData.CreationData.FemaleSkins.All(x => x != skin)
                             || CachedData.CreationData.FemaleFaces.All(x => x != face)
                             || CachedData.CreationData.FemaleHairs.All(x => x != hair)
                             || CachedData.CreationData.FemaleHairColors.All(x => x != hairColor)
                             || CachedData.CreationData.FemaleTops.All(x => x != topId)
                             || CachedData.CreationData.FemaleBottoms.All(x => x != bottomId)
                             || CachedData.CreationData.FemaleShoes.All(x => x != shoesId)
                             || CachedData.CreationData.FemaleWeapons.All(x => x != weaponId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }

            return error;
        }
    }
}
