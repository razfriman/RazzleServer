using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Network;
using RazzleServer.Login.Maple;
using RazzleServer.Common.Util;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Login
{
    public class LoginCenterClient : AClient
    {
        public LoginServer Server { get; set; }

        private readonly PendingKeyedQueue<string, bool> MigrationPool = new PendingKeyedQueue<string, bool>();
        private PendingKeyedQueue<string, bool> NameCheckPool = new PendingKeyedQueue<string, bool>();
        private PendingKeyedQueue<int, List<byte[]>> CharacterEntriesPool = new PendingKeyedQueue<int, List<byte[]>>();

        private PendingKeyedQueue<int, byte[]> CharacterCreationPool = new PendingKeyedQueue<int, byte[]>();

        public LoginCenterClient(LoginServer server, Socket session) : base(session, false)
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

                case InteroperabilityOperationCode.UpdateChannel:
                    UpdateChannel(packet);
                    break;

                case InteroperabilityOperationCode.UpdateChannelPopulation:
                    UpdateChannelPopulation(packet);
                    break;

                case InteroperabilityOperationCode.CharacterEntriesResponse:
                    GetCharacters(packet);
                    break;

                case InteroperabilityOperationCode.MigrationRegisterResponse:
                    Migrate(packet);
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
                Log.LogError($"Unable to register as Login Server: {response}");
                Server.ShutDown();
            }
        }

        private void UpdateChannel(PacketReader inPacket)
        {
            var worldID = inPacket.ReadByte();
            var add = inPacket.ReadBool();

            var world = Server.Worlds[worldID];

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

            Server.Worlds[worldID][channelID].Population = population;
        }

        private void CheckCharacterName(PacketReader inPacket)
        {
            string name = inPacket.ReadString();
            bool unusable = inPacket.ReadBool();

            NameCheckPool.Enqueue(name, unusable);
        }


        private void GetCharacters(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();

            List<byte[]> entires = new List<byte[]>();

            while (inPacket.Available > 0)
            {
                entires.Add(inPacket.ReadBytes(inPacket.ReadByte()));
            }

            CharacterEntriesPool.Enqueue(accountID, entires);
        }

        public List<byte[]> GetCharacters(byte worldID, int accountID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterEntriesRequest))
            {
                outPacket.WriteByte(worldID);
                outPacket.WriteInt(accountID);

                Send(outPacket);
            }

            return CharacterEntriesPool.Dequeue(accountID);
        }


        private void CreateCharacter(PacketReader inPacket)
        {
            int accountID = inPacket.ReadInt();
            byte[] characterData = inPacket.ReadBytes((int)inPacket.Available);

            CharacterCreationPool.Enqueue(accountID, characterData);
        }

        public bool Migrate(string host, int accountID, int characterID)
        {
            using (var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRegisterRequest))
            {
                outPacket.WriteString(host);
                outPacket.WriteInt(accountID);
                outPacket.WriteInt(characterID);

                Send(outPacket);
            }

            return MigrationPool.Dequeue(host);
        }

        private void Migrate(PacketReader inPacket)
        {
            string host = inPacket.ReadString();
            bool valid = inPacket.ReadBool();

            MigrationPool.Enqueue(host, valid);
        }
    }
}
