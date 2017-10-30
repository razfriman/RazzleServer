﻿using System.Net.Sockets;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Network;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Common.Util;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Game
{
    public class GameCenterClient : AClient
    {
        ChannelServer Server { get; set; }
        public GameCenterClient(ChannelServer server, Socket session) : base(session)
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


        public void Initialize(params object[] args)
        {
            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Channel);
            pw.WriteString((string)args[0]);
            Send(pw);
        }


        private void Register(PacketReader inPacket)
        {
            var response = (ServerRegistrationResponse)inPacket.ReadByte();

            switch (response)
            {
                case ServerRegistrationResponse.Valid:
                    {
                        Server.WorldID = inPacket.ReadByte();
                        Server.WorldName = inPacket.ReadString();
                        Server.TickerMessage = inPacket.ReadString();
                        Server.ChannelID = inPacket.ReadByte();
                        Server.RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, inPacket.ReadUShort());
                        Server.Listen();
                        Server.AllowMultiLeveling = inPacket.ReadBool();

                        Log.LogInformation("Characters will {0}be able to continuously level-up.", Server.AllowMultiLeveling ? "" : "not ");

                        Server.ExperienceRate = inPacket.ReadInt();
                        Server.QuestExperienceRate = inPacket.ReadInt();
                        Server.PartyQuestExperienceRate = inPacket.ReadInt();
                        Server.MesoRate = inPacket.ReadInt();
                        Server.DropRate = inPacket.ReadInt();

                        Log.Success("Registered Channel Server ({0} [{1}]-{2}).", Server.WorldName, Server.WorldID, Server.ChannelID);
                    }
                    break;

                default:
                    {
                        Log.Error("Unable to register as Channel Server: {0}", ServerRegistrationResponseResolver.Explain(response));

                        Server.Stop();
                    }
                    break;
            }

            Server.CenterConnectionDone.Set();
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

                foreach (Datum datum in new Datums("characters").PopulateWith("ID", "AccountID = {0} AND WorldID = {1}", accountID, Server.WorldID))
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
            JobType jobType = (JobType)inPacket.ReadInt();
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
                if (!DataProvider.CreationData.MaleSkins.Any(x => x.Item1 == jobType && x.Item2 == skin)
                    || !DataProvider.CreationData.MaleFaces.Any(x => x.Item1 == jobType && x.Item2 == face)
                    || !DataProvider.CreationData.MaleHairs.Any(x => x.Item1 == jobType && x.Item2 == hair)
                    || !DataProvider.CreationData.MaleHairColors.Any(x => x.Item1 == jobType && x.Item2 == hairColor)
                    || !DataProvider.CreationData.MaleTops.Any(x => x.Item1 == jobType && x.Item2 == topID)
                    || !DataProvider.CreationData.MaleBottoms.Any(x => x.Item1 == jobType && x.Item2 == bottomID)
                    || !DataProvider.CreationData.MaleShoes.Any(x => x.Item1 == jobType && x.Item2 == shoesID)
                    || !DataProvider.CreationData.MaleWeapons.Any(x => x.Item1 == jobType && x.Item2 == weaponID))
                {
                    error = true;
                }
            }
            else if (gender == Gender.Female)
            {
                if (!DataProvider.CreationData.FemaleSkins.Any(x => x.Item1 == jobType && x.Item2 == skin)
                    || !DataProvider.CreationData.FemaleFaces.Any(x => x.Item1 == jobType && x.Item2 == face)
                    || !DataProvider.CreationData.FemaleHairs.Any(x => x.Item1 == jobType && x.Item2 == hair)
                    || !DataProvider.CreationData.FemaleHairColors.Any(x => x.Item1 == jobType && x.Item2 == hairColor)
                    || !DataProvider.CreationData.FemaleTops.Any(x => x.Item1 == jobType && x.Item2 == topID)
                    || !DataProvider.CreationData.FemaleBottoms.Any(x => x.Item1 == jobType && x.Item2 == bottomID)
                    || !DataProvider.CreationData.FemaleShoes.Any(x => x.Item1 == jobType && x.Item2 == shoesID)
                    || !DataProvider.CreationData.FemaleWeapons.Any(x => x.Item1 == jobType && x.Item2 == weaponID))
                {
                    error = true;
                }
            }
            else // NOTE: Not allowed to choose "both" genders at character creation.
            {
                error = true;
            }

            Character character = new Character();

            character.AccountID = accountID;
            character.WorldID = Server.WorldID;
            character.Name = name;
            character.Gender = gender;
            character.Skin = skin;
            character.Face = face;
            character.Hair = hair + hairColor;
            character.Level = 1;
            character.Job = jobType == JobType.Cygnus ? Job.Noblesse : jobType == JobType.Explorer ? Job.Beginner : Job.Legend;
            character.Strength = 12;
            character.Dexterity = 5;
            character.Intelligence = 4;
            character.Luck = 4;
            character.MaxHealth = 50;
            character.MaxMana = 5;
            character.Health = 50;
            character.Mana = 5;
            character.AbilityPoints = 0;
            character.SkillPoints = 0;
            character.Experience = 0;
            character.Fame = 0;
            character.Map = DataProvider.Maps[jobType == JobType.Cygnus ? 130030000 : jobType == JobType.Explorer ? 10000 : 914000000];
            character.SpawnPoint = 0;
            character.Meso = 0;

            character.Items.Add(new Item(topID, equipped: true));
            character.Items.Add(new Item(bottomID, equipped: true));
            character.Items.Add(new Item(shoesID, equipped: true));
            character.Items.Add(new Item(weaponID, equipped: true));
            character.Items.Add(new Item(jobType == JobType.Cygnus ? 4161047 : jobType == JobType.Explorer ? 4161001 : 4161048), forceGetSlot: true);

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

            character.Save();

            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterCreationResponse))
            {
                outPacket.WriteInt(accountID);
                outPacket.WriteBytes(character.ToByteArray());

                Send(outPacket);
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

        private PendingKeyedQueue<string, int> MigrationValidationPool = new PendingKeyedQueue<string, int>();



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
