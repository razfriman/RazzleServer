using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using System;
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
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray().AsSpan();
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
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray().AsSpan();
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
            cryptoInstance.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray().AsSpan();
            cryptoInstance.Encrypt(ref encryptedPacket, true);

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
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray().AsSpan();
            cryptoInstance.Encrypt(ref encryptedPacket, false);

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
            var encryptedPacket = packet.ToArray().AsSpan();
            encryptor.Encrypt(ref encryptedPacket, true);

            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIv(iv);
            var decryptedPacket = encryptedPacket.ToArray().AsSpan();
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
            encryptor.SetIv(iv);

            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            var originalPacket = packet.ToArray();
            var encryptedPacket = packet.ToArray().AsSpan();
            encryptor.Encrypt(ref encryptedPacket, false);

            var decryptor = new MapleCipher(version, aesKey);
            decryptor.SetIv(iv);
            var decryptedPacket = encryptedPacket.ToArray().AsSpan();
            decryptor.Decrypt(ref decryptedPacket);

            Assert.AreEqual(originalPacket.ByteArrayToString(), decryptedPacket.ByteArrayToString());
        }

       
        private void DoSomethingM(Memory<byte> memory)
        {
            var span = memory.Span;
            span[0] = 1;
            span[1] = 2;
            span[2] = 3;
            span[3] = 4;
        }

        private void DoSomethingS(Span<byte> span)
        {
            span[0] = 1;
            span[1] = 2;
            span[2] = 3;
            span[3] = 4;
        }

        [TestMethod]
        public void TestSpanDataPassage()
        {
            var memory = new byte[4].AsMemory();
            DoSomethingM(memory);
            Assert.AreEqual("01 02 03 04", memory.ByteArrayToString());

            memory = new byte[4].AsMemory();
            DoSomethingS(memory.Span);
            Assert.AreEqual("01 02 03 04", memory.ByteArrayToString());
        }

        [TestMethod]
        public void TestMemoryDataPassage()
        {
            var memory = new byte[4].AsSpan();
            DoSomethingS(memory);
            Assert.AreEqual("01 02 03 04", memory.ByteArrayToString());
        }

        [TestMethod]
        public void TestByteMDataPassage()
        {
            var memory = new byte[4];
            DoSomethingM(memory);
            Assert.AreEqual("01 02 03 04", memory.ByteArrayToString());
        }

        [TestMethod]
        public void TestByteSDataPassage()
        {
            var memory = new byte[4];
            DoSomethingS(memory);
            Assert.AreEqual("01 02 03 04", memory.ByteArrayToString());
        }
    }
}
