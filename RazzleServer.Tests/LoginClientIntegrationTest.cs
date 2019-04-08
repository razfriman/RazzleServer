using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;
using RazzleServer.Tests.Util;

namespace RazzleServer.Tests
{
    [TestClass]
    public class LoginClientIntegrationTest
    {
        private const int Timeout = 2000;

        private FakeServerManager _server;

        [TestInitialize]
        public async Task Setup()
        {
            _server = new FakeServerManager();
            await _server.StartAsync(CancellationToken.None);
        }

        [TestMethod]
        public void FakeServerManager_LoginServer_Succeeds()
        {
            Assert.IsNotNull(_server.Login);
        }

        [TestMethod]
        public void FakeServerManager_GameServer_Succeeds()
        {
            Assert.IsNotNull(_server.Worlds[0][0]);
        }

        [TestMethod]
        public void Login_EnableAutoRegister_CreatesAccount()
        {
            ServerConfig.Instance.EnableAutoRegister = true;
            var client = _server.AddFakeLoginClient();
            var username = "admin";
            var password = "admin";
            SendLogin(client, username, password, LoginResult.InvalidUsername);
            SendLogin(client, username, password, LoginResult.Valid);
        }

        [TestMethod]
        public void Login_NoEnableAutoRegister_ReturnsInvalidUsername()
        {
            ServerConfig.Instance.EnableAutoRegister = false;
            var client = _server.AddFakeLoginClient();
            var username = "admin";
            var password = "admin";
            SendLogin(client, username, password, LoginResult.InvalidUsername);
            SendLogin(client, username, password, LoginResult.InvalidUsername);
        }

        [TestMethod]
        public void Login_WrongPassword_ReturnsInvalidPassword()
        {
            ServerConfig.Instance.EnableAutoRegister = true;
            var client = _server.AddFakeLoginClient();
            var username = "admin";
            var password = "admin";
            SendLogin(client, username, password, LoginResult.InvalidUsername);
            SendLogin(client, username, password, LoginResult.Valid);
            SendLogin(client, username, "xxxxxx", LoginResult.InvalidPassword);
        }

        [TestMethod]
        public void Login_Valid_SendsWorldStatus()
        {
            ServerConfig.Instance.EnableAutoRegister = true;
            var client = _server.AddFakeLoginClient();
            var username = "admin";
            var password = "admin";
            SendLogin(client, username, password, LoginResult.InvalidUsername);
            SendLogin(client, username, password, LoginResult.Valid);
            CheckWorldInformation(client, false);    
            CheckWorldInformation(client, true);
        }

        private void CheckWorldInformation(FakeLoginClient client, bool isEnd)
        {
            var packetEvent = new ManualResetEventSlim();

            void WorldInformationHandler(PacketReader packet)
            {
                if (packetEvent.IsSet)
                {
                    return;
                }
                
                var header = (ServerOperationCode)packet.ReadByte();
                Assert.AreEqual(ServerOperationCode.WorldInformation, header);
                
                var worldId = packet.ReadByte();

                if (isEnd)
                {
                    Assert.AreEqual(0xFF, worldId);
                }
                else
                {
                    Assert.IsTrue(client.Server.Manager.Worlds.Contains(worldId));
                    var world = client.Server.Manager.Worlds[worldId];
                    var worldName = packet.ReadString();
                    var channels = packet.ReadByte();
                    
                    Assert.AreEqual(world.Name, worldName);
                    Assert.AreEqual(world.Channels, channels);
                }

                packetEvent.Set();
                client.ServerToClientPacket -= WorldInformationHandler;
            }

            client.ServerToClientPacket += WorldInformationHandler;
            packetEvent.Wait(Timeout);
        }

        private static void SendLogin(FakeLoginClient client, string username, string password,
            LoginResult expectedResult)
        {
            var loginEvent = new ManualResetEventSlim();

            void LoginHandler(PacketReader packet)
            {
                if (loginEvent.IsSet)
                {
                    return;
                }

                var header = (ServerOperationCode)packet.ReadByte();
                var actualResult = (LoginResult)packet.ReadByte();

                Assert.AreEqual(ServerOperationCode.CheckPasswordResult, header);
                Assert.AreEqual(expectedResult, actualResult);

                loginEvent.Set();
                client.ServerToClientPacket -= LoginHandler;
            }

            client.ServerToClientPacket += LoginHandler;


            var pw = new PacketWriter();
            pw.WriteByte((ClientOperationCode.Login));
            pw.WriteString(username);
            pw.WriteString(password);
            pw.WriteZeroBytes(4);
            pw.WriteZeroBytes(16);
            client.Receive(new PacketReader(pw.ToArray()));

            loginEvent.Wait(Timeout);
        }
    }
}
