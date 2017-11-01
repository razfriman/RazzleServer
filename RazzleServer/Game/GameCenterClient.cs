using System.Net.Sockets;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Network;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Common.Util;
using Microsoft.Extensions.Logging;

namespace RazzleServer.Game
{
    public class GameCenterClient : AClient
    {
        public GameServer Server { get; set; }

        private readonly PendingKeyedQueue<string, int> MigrationValidationPool = new PendingKeyedQueue<string, int>();


        public GameCenterClient(GameServer server, Socket session) : base(session, false)
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

                case InteroperabilityOperationCode.CharacterEntriesRequest:
                    SendCharacters(packet);
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
