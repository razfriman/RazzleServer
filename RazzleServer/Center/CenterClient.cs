using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MapleLib.PacketLib;
using Microsoft.Extensions.Logging;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Util;

namespace RazzleServer.Center
{
    public sealed class CenterClient : AClient
    {
        public static string SecurityCode { get; set; }
        private static ILogger Log = LogManager.Log;

        public ServerType Type { get; private set; }

        public World World { get; private set; }
        public byte ID { get; set; }
        public int Population { get; private set; }
        public CenterServer Server { get; private set; }

        public CenterClient(Socket socket, CenterServer server) : base(socket)
        {
            Server = server;
        }

        public override void Register()
        {
            Server.AddClient(this);
        }

        public override void Terminate(string message = null)
        {
            switch (Type)
            {
                case ServerType.Login:
                    {
                        WvsCenter.Login = null;

                        Log.LogWarning("Unregistered Login Server.");
                    }
                    break;

                case ServerType.Channel:
                    {
                        World.Remove(this);

                        using (PacketReader Packet = new Packet(InteroperabilityOperationCode.UpdateChannel))
                        {
                            Packet.WriteByte(World.ID);
                            Packet.WriteBool(false);
                            Packet.WriteByte(ID);

                            WvsCenter.Login?.Send(Packet);
                        }

                        Log.LogWarning("Unregistered Channel Server ({0}-{1}).", World.Name, ID);
                    }
                    break;

                case ServerType.Shop:
                    {
                        World.Shop = null;

                        Log.LogWarning("Unregistered Shop Server ({0}).", World.Name);
                        break;
                    }
            }
        }

        public override void Unregister()
        {
            Server.RemoveClient(this);
        }

        private void Migrate(PacketReader inPacket)
        {
            string host = inPacket.ReadString();
            int accountID = inPacket.ReadInt();
            int characterID = inPacket.ReadInt();

            var valid = false;

            if (!WvsCenter.Migrations.Contains(host))
            {
                valid = true;
                WvsCenter.Migrations.Add(new Migration(host, accountID, characterID));
            }

            var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRegisterResponse);
            outPacket.WriteString(host);
            outPacket.WriteBool(valid);
            Send(outPacket);
        }

       
        public override void Receive(PacketReader packet)
        {
            var header = packet.ReadUShort();
            // look for handlers
        }
    }
}
