using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Network;
using RazzleServer.Login.Maple;
using RazzleServer.Common.Util;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Net;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public class LoginCenterClient : AClient
    {

        public LoginServer Server { get; set; }

        public LoginCenterClient(LoginServer server, Socket session) : base(session)
        {
            Server = server;
        }

        public void Initialize(params object[] args)
        {
            using (var Packet = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest))
            {
                Packet.WriteByte((byte)ServerType.Login);
                Packet.WriteString((string)args[0]);
                Packet.WriteByte((byte)Server.Worlds.Count);

                foreach (World loopWorld in Server.Worlds)
                {
                    Packet.WriteByte(loopWorld.ID);
                    Packet.WriteString(loopWorld.Name);
                    Packet.WriteUShort(loopWorld.Port);
                    Packet.WriteUShort(loopWorld.ShopPort);
                    Packet.WriteByte(loopWorld.Count);
                    Packet.WriteString(loopWorld.TickerMessage);
                    Packet.WriteBool(loopWorld.AllowMultiLeveling);
                    Packet.WriteInt(loopWorld.ExperienceRate);
                    Packet.WriteInt(loopWorld.QuestExperienceRate);
                    Packet.WriteInt(loopWorld.PartyQuestExperienceRate);
                    Packet.WriteInt(loopWorld.MesoRate);
                    Packet.WriteInt(loopWorld.DropRate);
                }

                this.Send(Packet);
            }
        }

        public override void Receive(PacketReader packet)
        {
            var header = (InteroperabilityOperationCode)packet.ReadUShort();
            switch (header)
            {
                case InteroperabilityOperationCode.RegistrationResponse:
                    this.Register(packet);
                    break;

                case InteroperabilityOperationCode.UpdateChannel:
                    this.UpdateChannel(packet);
                    break;

                case InteroperabilityOperationCode.UpdateChannelPopulation:
                    this.UpdateChannelPopulation(packet);
                    break;

                case InteroperabilityOperationCode.CharacterNameCheckResponse:
                    this.CheckCharacterName(packet);
                    break;

                case InteroperabilityOperationCode.CharacterEntriesResponse:
                    this.GetCharacters(packet);
                    break;

                case InteroperabilityOperationCode.CharacterCreationResponse:
                    this.CreateCharacter(packet);
                    break;

                case InteroperabilityOperationCode.MigrationRegisterResponse:
                    this.Migrate(packet);
                    break;
            }
        }

        private void Register(PacketReader inPacket)
        {
            var response = (ServerRegistrationResponse)inPacket.ReadByte();

            switch (response)
            {
                case ServerRegistrationResponse.Valid:
                    {
                        byte[] loginIp = { 0, 0, 0, 0 };
                        Server.Start(new IPAddress(loginIp), ServerConfig.Instance.LoginPort);
                        Log.LogInformation("Registered Login Server.");
                    }
                    break;

                default:
                    {
                        Log.LogError(response);
                        Server.ShutDown();
                    }
                    break;
            }
        }

        private void UpdateChannel(PacketReader inPacket)
        {
            var worldID = inPacket.ReadByte();
            var add = inPacket.ReadBool();

            var world = WvsLogin.Worlds[worldID];

            if (add)
            {
                world.Add(new Channel(inPacket));
            }
            else
            {
                byte channelID = inPacket.ReadByte();

                world.Remove(channelID);
            }
        }

        private void UpdateChannelPopulation(PacketReader inPacket)
        {
            byte worldID = inPacket.ReadByte();
            byte channelID = inPacket.ReadByte();
            int population = inPacket.ReadInt();

            WvsLogin.Worlds[worldID][channelID].Population = population;
        }

        private void CheckCharacterName(PacketReader inPacket)
        {
            string name = inPacket.ReadString();
            bool unusable = inPacket.ReadBool();

            this.NameCheckPool.Enqueue(name, unusable);
        }

        private PendingKeyedQueue<int, List<byte[]>> CharacterEntriesPool = new PendingKeyedQueue<int, List<byte[]>>();

        private void GetCharacters(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();

            List<byte[]> entires = new List<byte[]>();

            while (inPacket.Remaining > 0)
            {
                entires.Add(inPacket.ReadBytes(inPacket.ReadByte()));
            }

            this.CharacterEntriesPool.Enqueue(accountID, entires);
        }

        public List<byte[]> GetCharacters(byte worldID, int accountID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterEntriesRequest))
            {
                outPacket.WriteByte(worldID);
                outPacket.WriteInt(accountID);

                this.Send(outPacket);
            }

            return this.CharacterEntriesPool.Dequeue(accountID);
        }

        private PendingKeyedQueue<string, bool> NameCheckPool = new PendingKeyedQueue<string, bool>();

        public bool IsNameTaken(string name)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterNameCheckRequest))
            {
                outPacket.WriteString(name);

                this.Send(outPacket);
            }

            return this.NameCheckPool.Dequeue(name);
        }

        private PendingKeyedQueue<int, byte[]> CharacterCreationPool = new PendingKeyedQueue<int, byte[]>();

        private void CreateCharacter(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();
            byte[] characterData = inPacket.ReadBytes(inPacket.Available);

            this.CharacterCreationPool.Enqueue(accountID, characterData);
        }

        public byte[] CreateCharacter(byte worldID, int accountID, byte[] characterData)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterCreationRequest))
            {
                outPacket.WriteByte(worldID);
                outPacket.WriteInt(accountID);
                outPacket.WriteBytes(characterData);

                this.Send(outPacket);
            }

            return this.CharacterCreationPool.Dequeue(accountID);
        }

        private PendingKeyedQueue<string, bool> MigrationPool = new PendingKeyedQueue<string, bool>();



        public bool Migrate(string host, int accountID, int characterID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRegisterRequest))
            {
                outPacket
                    .WriteString(host)
                    .WriteInt(accountID)
                    .WriteInt(characterID);

                this.Send(outPacket);
            }

            return this.MigrationPool.Dequeue(host);
        }

        private void Migrate(PacketReader inPacket)
        {
            string host = inPacket.ReadString();
            bool valid = inPacket.ReadBool();

            this.MigrationPool.Enqueue(host, valid);
        }
    }
}
