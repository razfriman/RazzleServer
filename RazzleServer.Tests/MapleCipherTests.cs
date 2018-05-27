using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using System.Linq;

namespace RazzleServer.Tests
{
    [TestClass]
    public class MapleCipherTests
    {
        [TestMethod]
        public void GetPacketLength_ToClient_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            cryptoInstance.Encrypt(ref encryptedPacket, true);
            var decryptedLength = cryptoInstance.GetPacketLength(encryptedPacket);
            Assert.AreEqual(originalPacket.Length, decryptedLength);
        }


        [TestMethod]
        public void GetPacketLength_ToServer_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            cryptoInstance.Encrypt(ref encryptedPacket, false);
            var decryptedLength = cryptoInstance.GetPacketLength(encryptedPacket);
            Assert.AreEqual(originalPacket.Length, decryptedLength);
        }

        [TestMethod]
        public void CheckHeaderToClient_Valid_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            cryptoInstance.Encrypt(ref encryptedPacket, true);

            var checkCrypto = new MapleCipher(version, aesKey);
            checkCrypto.SetIV(iv);
            Assert.IsTrue(checkCrypto.CheckHeaderToClient(encryptedPacket));
        }

        [TestMethod]
        public void CheckHeaderToServer_Valid_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            cryptoInstance.Encrypt(ref encryptedPacket, false);

            var checkCrypto = new MapleCipher(version, aesKey);
            checkCrypto.SetIV(iv);
            Assert.IsTrue(checkCrypto.CheckHeaderToServer(encryptedPacket));
        }

        [TestMethod]
        public void EncryptDecrypt_ToClient_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            encryptor.Encrypt(ref encryptedPacket, true);

            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIV(iv);
            var decryptedPacket = encryptedPacket.ToArray();
            decryptor.Decrypt(ref decryptedPacket);

            Assert.AreEqual(originalPacket.ByteArrayToString(), decryptedPacket.ByteArrayToString());
        }

        [TestMethod]
        public void EncryptDecrypt_ToServer_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIV(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray();
            encryptor.Encrypt(ref encryptedPacket, false);

            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIV(iv);
            var decryptedPacket = encryptedPacket.ToArray();
            decryptor.Decrypt(ref decryptedPacket);

            Assert.AreEqual(originalPacket.ByteArrayToString(), decryptedPacket.ByteArrayToString());
        }
    }
}
