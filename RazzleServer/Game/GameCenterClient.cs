using System.Net.Sockets;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Network;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Common.Util;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Game
{
    public class GameCenterClient : AClient
    {
        public GameServer Server { get; set; }

        private readonly PendingKeyedQueue<string, int> MigrationValidationPool = new PendingKeyedQueue<string, int>();


        public GameCenterClient(GameServer server, Socket session) : base(session)
        {
            Server = server;
        }

        public override void Receive(PacketReader packet)
        {
            var header = (InteroperabilityOperationCode)packet.ReadUShort();
            switch (header)
            {
                case InteroperabilityOperationCode.RegistrationResponse:
                    Register(packet);
                    break;

                case InteroperabilityOperationCode.UpdateChannelID:
                    UpdateChannelID(packet);
                    break;

                case InteroperabilityOperationCode.CharacterNameCheckRequest:
                    CheckCharacterName(packet);
                    break;

                case InteroperabilityOperationCode.CharacterEntriesRequest:
                    SendCharacters(packet);
                    break;

                case InteroperabilityOperationCode.CharacterCreationRequest:
                    CreateCharacter(packet);
                    break;

                case InteroperabilityOperationCode.MigrationResponse:
                    Migrate(packet);
                    break;

                case InteroperabilityOperationCode.ChannelPortResponse:
                    ChannelPortResponse(packet);
                    break;
            }
        }

        private void Register(PacketReader inPacket)
        {
            var response = (ServerRegistrationResponse)inPacket.ReadByte();

            if (response == ServerRegistrationResponse.Valid)
            {
                Server.ServerRegistered();
            }
            else
            {
                Log.LogError($"Unable to register as Channel Server: {response}");
                Server.ShutDown();
            }
        }

        private void UpdateChannelID(PacketReader inPacket)
        {
            Server.ChannelID = inPacket.ReadByte();
        }

        private void CheckCharacterName(PacketReader inPacket)
        {
            string name = inPacket.ReadString();
            bool unusable = Database.Exists("characters", "Name = {0}", name);

            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterNameCheckResponse))
            {
                outPacket.WriteString(name);
                outPacket.WriteBool(unusable);

                Send(outPacket);
            }
        }

        private void SendCharacters(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();

            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterEntriesResponse))
            {
                outPacket.WriteInt(accountID);

                foreach (Datum datum in new Datums("characters").PopulateWith("ID", "AccountID = {0} AND WorldID = {1}", accountID, Server.World.ID))
                {
                    Character character = new Character((int)datum["ID"]);
                    character.Load();

                    byte[] entry = character.ToByteArray();

                    outPacket.WriteByte((byte)entry.Length);
                    outPacket.WriteBytes(entry);
                }

                Send(outPacket);
            }
        }

        // TODO: Name & items validation.
        private void CreateCharacter(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();
            string name = inPacket.ReadString();
            int face = inPacket.ReadInt();
            int hair = inPacket.ReadInt();
            int hairColor = inPacket.ReadInt();
            byte skin = (byte)inPacket.ReadInt();
            int topID = inPacket.ReadInt();
            int bottomID = inPacket.ReadInt();
            int shoesID = inPacket.ReadInt();
            int weaponID = inPacket.ReadInt();
            Gender gender = (Gender)inPacket.ReadByte();

            bool error = false;

            if (name.Length < 4 || name.Length > 12
                || Database.Exists("characters", "Name = {0}", name)
                || DataProvider.CreationData.ForbiddenNames.Any(forbiddenWord => name.ToLowerInvariant().Contains(forbiddenWord)))
            {
                error = true;
            }

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
                WorldID = Server.World.ID,
                Name = name,
                Gender = gender,
                Skin = skin,
                Face = face,
                Hair = hair + hairColor,
                Level = 1,
                Job =Job.Beginner,
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
            character.Keymap.Add(new Shortcut(KeymapKey.Five, KeymapAction.AllianceChat));
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
            character.Keymap.Add(new Shortcut(KeymapKey.F, KeymapAction.FamilyList));
            character.Keymap.Add(new Shortcut(KeymapKey.G, KeymapAction.GuildList));
            character.Keymap.Add(new Shortcut(KeymapKey.H, KeymapAction.WhisperChat));
            character.Keymap.Add(new Shortcut(KeymapKey.K, KeymapAction.SkillMenu));
            character.Keymap.Add(new Shortcut(KeymapKey.L, KeymapAction.QuestHelper));
            character.Keymap.Add(new Shortcut(KeymapKey.Semicolon, KeymapAction.Medal));
            character.Keymap.Add(new Shortcut(KeymapKey.Quote, KeymapAction.ExpandChat));
            character.Keymap.Add(new Shortcut(KeymapKey.Backtick, KeymapAction.CashShop));
            character.Keymap.Add(new Shortcut(KeymapKey.Backslash, KeymapAction.SetKey));
            character.Keymap.Add(new Shortcut(KeymapKey.Z, KeymapAction.PickUp));
            character.Keymap.Add(new Shortcut(KeymapKey.X, KeymapAction.Sit));
            character.Keymap.Add(new Shortcut(KeymapKey.C, KeymapAction.Messenger));
            character.Keymap.Add(new Shortcut(KeymapKey.B, KeymapAction.MonsterBook));
            character.Keymap.Add(new Shortcut(KeymapKey.M, KeymapAction.MiniMap));
            character.Keymap.Add(new Shortcut(KeymapKey.LeftAlt, KeymapAction.Jump));
            character.Keymap.Add(new Shortcut(KeymapKey.Space, KeymapAction.NpcChat));
            character.Keymap.Add(new Shortcut(KeymapKey.F1, KeymapAction.Cockeyed));
            character.Keymap.Add(new Shortcut(KeymapKey.F2, KeymapAction.Happy));
            character.Keymap.Add(new Shortcut(KeymapKey.F3, KeymapAction.Sarcastic));
            character.Keymap.Add(new Shortcut(KeymapKey.F4, KeymapAction.Crying));
            character.Keymap.Add(new Shortcut(KeymapKey.F5, KeymapAction.Outraged));
            character.Keymap.Add(new Shortcut(KeymapKey.F6, KeymapAction.Shocked));
            character.Keymap.Add(new Shortcut(KeymapKey.F7, KeymapAction.Annoyed));

            if (!error)
            {
                character.Save();

                using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterCreationResponse))
                {
                    outPacket.WriteInt(accountID);
                    outPacket.WriteBytes(character.ToByteArray());

                    Send(outPacket);
                }
            }
            else
            {
                Log.LogError("Error when trying to create character");
            }
        }

        private void ChannelPortResponse(PacketReader inPacket)
        {
            byte id = inPacket.ReadByte();
            ushort port = inPacket.ReadUShort();

            ChannelPortPool.Enqueue(id, port);
        }

        public void UpdatePopulation(int population)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.UpdateChannelPopulation))
            {
                outPacket.WriteInt(population);

                Send(outPacket);
            }
        }

        private PendingKeyedQueue<byte, ushort> ChannelPortPool = new PendingKeyedQueue<byte, ushort>();

        public ushort GetChannelPort(byte channelID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.ChannelPortRequest))
            {
                outPacket.WriteByte(channelID);

                Send(outPacket);
            }

            return ChannelPortPool.Dequeue(channelID);
        }

        public int ValidateMigration(string host, int characterID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRequest))
            {
                outPacket.WriteString(host);
                outPacket.WriteInt(characterID);
                Send(outPacket);
            }

            return MigrationValidationPool.Dequeue(host);
        }

        private void Migrate(PacketReader inPacket)
        {
            string host = inPacket.ReadString();
            int accountID = inPacket.ReadInt();

            MigrationValidationPool.Enqueue(host, accountID);
        }
    }
}
