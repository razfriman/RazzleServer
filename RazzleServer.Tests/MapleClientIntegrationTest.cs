using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Login;
using RazzleServer.Net;
using RazzleServer.Net.Packet;

namespace RazzleServer.Tests
{
    [TestClass]
    public class MapleClientIntegrationTest
    {
        private FakeServerManager ServerManager { get; set; }

        [TestInitialize]
        public async Task Setup()
        {
            ServerManager = new FakeServerManager();
            await ServerManager.StartAsync(CancellationToken.None);
        }

        [TestMethod]
        public void ValidateServersCreated()
        {
            Assert.IsNotNull(ServerManager.Login);
            Assert.IsNotNull(ServerManager.Worlds[0][0]);
        }

        [TestMethod]
        public void LoginTwice_CreatesAccount()
        {
            var fakeLoginClient = ServerManager.AddFakeLoginClient();

            var gotIsInvalidUsername = false;
            
            fakeLoginClient.ServerToClientPacket += packet =>
            {
                var header = (ClientOperationCode) packet.ReadByte();

                if (header == ClientOperationCode.Login)
                {
                    var result = (LoginResult) packet.ReadByte();
                    
                    if (gotIsInvalidUsername)
                    {
                        Assert.AreEqual(LoginResult.Valid, result);
                    }
                    
                    if (result == LoginResult.InvalidUsername && !gotIsInvalidUsername)
                    {
                        gotIsInvalidUsername = true;
                    }
                }
            };
            
            var pw = new PacketWriter();
            pw.WriteByte((ClientOperationCode.Login));
            pw.WriteString("admin");
            pw.WriteString("admin");
            pw.WriteZeroBytes(4);
            pw.WriteZeroBytes(16);
            fakeLoginClient.Receive(new PacketReader(pw.ToArray()));
            fakeLoginClient.Receive(new PacketReader(pw.ToArray()));
        }
    }
}
