using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Util;
using System;
using System.Threading;
using RazzleServer.Crypto;
using RazzleServer.Net.Packet;

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
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = cryptoInstance.Encrypt(packet.ToArray().AsSpan(), true);
            var decryptedLength = MapleCipher.GetPacketLength(encryptedPacket);
            Assert.AreEqual(originalPacket.Length, decryptedLength);
        }


        [TestMethod]
        public void GetPacketLength_ToServer_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = cryptoInstance.Encrypt(packet.ToArray().AsSpan(), false);
            var decryptedLength = MapleCipher.GetPacketLength(encryptedPacket);
            Assert.AreEqual(originalPacket.Length, decryptedLength);
        }

        [TestMethod]
        public void CheckHeaderToClient_Valid_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var encryptedPacket = cryptoInstance.Encrypt(packet.ToArray().AsSpan(), true);
            var checkCrypto = new MapleCipher(version, aesKey);
            checkCrypto.SetIv(iv);
            Assert.IsTrue(checkCrypto.CheckHeaderToClient(encryptedPacket));
        }

        [TestMethod]
        public void CheckHeaderToServer_Valid_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var cryptoInstance = new MapleCipher(version, aesKey);
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var encryptedPacket = cryptoInstance.Encrypt(packet.ToArray().AsSpan(), false);
            var checkCrypto = new MapleCipher(version, aesKey);
            checkCrypto.SetIv(iv);
            Assert.IsTrue(checkCrypto.CheckHeaderToServer(encryptedPacket));
        }

        [TestMethod]
        public void EncryptDecrypt_ToClient_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = encryptor.Encrypt(packet.ToArray().AsSpan(), true);

            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIv(iv);
            var decryptedPacket = decryptor.Decrypt(encryptedPacket.ToArray().AsSpan());

            Assert.AreEqual(originalPacket.ByteArrayToString(), decryptedPacket.ByteArrayToString());
        }

        [TestMethod]
        public void EncryptDecrypt_ToServer_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = encryptor.Encrypt(packet.ToArray().AsSpan(), false);
            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIv(iv);
            var decryptedPacket = decryptor.Decrypt(encryptedPacket);
            Assert.AreEqual(originalPacket.ByteArrayToString(), decryptedPacket.ByteArrayToString());
        }

        [TestMethod]
        public void EncryptDecryptProvider_ToServer_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = encryptor.Encrypt(packet.ToArray().AsSpan(), false);


            var waiter = new ManualResetEventSlim();

            var provider = new MapleCipherProvider(version, aesKey);
            provider.SetVectors(0, 0);
            var buffer = encryptedPacket.ToArray().AsMemory();
            provider.PacketFinished += received =>
            {
                Assert.AreEqual(originalPacket.Length, received.Length, "Packet length should match");
                Assert.AreEqual(originalPacket.ByteArrayToString(), received.ByteArrayToString());
                waiter.Set();
            };
            provider.AddData(buffer, 0, encryptedPacket.Length);

            waiter.Wait();
        }

        [TestMethod]
        public void EncryptDecryptProvider_ToClient_Succeeds()
        {
            var version = (ushort)55;
            var aesKey = (ulong)0x52330F1BB4060813;
            var iv = (uint)0;
            var encryptor = new MapleCipher(version, aesKey);
            encryptor.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = encryptor.Encrypt(packet.ToArray().AsSpan(), true);


            var waiter = new ManualResetEventSlim();

            var provider = new MapleCipherProvider(version, aesKey, toClient: false);
            provider.SetVectors(0, 0);
            var buffer = encryptedPacket.ToArray().AsMemory();
            provider.PacketFinished += received =>
            {
                Assert.AreEqual(originalPacket.Length, received.Length, "Packet length should match");
                Assert.AreEqual(originalPacket.ByteArrayToString(), received.ByteArrayToString());
                waiter.Set();
            };
            provider.AddData(buffer, 0, encryptedPacket.Length);

            waiter.Wait();
        }
    }
}
