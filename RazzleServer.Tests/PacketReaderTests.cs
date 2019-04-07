using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Tests
{
    [TestClass]
    public class PacketReaderTests
    {
        [TestMethod]
        public void EmptyPacketReader_Succeeds()
        {
            using (var packet = new PacketReader(new byte[] { }))
            {
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00", (byte)0)]
        [DataRow("01", (byte)1)]
        [DataRow("7F", (byte)sbyte.MaxValue)]
        [DataRow("80", (byte)((byte)sbyte.MaxValue + 1))]
        [DataRow("FE", (byte)(byte.MaxValue - 1))]
        [DataRow("FF", byte.MaxValue)]
        public void ReadByte_Succeeds(string input, byte expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadByte();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00", (sbyte)0)]
        [DataRow("01", (sbyte)1)]
        [DataRow("7F", sbyte.MaxValue)]
        [DataRow("80", sbyte.MinValue)]
        [DataRow("FE", (sbyte)-2)]
        [DataRow("FF", (sbyte)-1)]
        public void ReadSByte_Succeeds(string input, sbyte expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadSByte();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00", (short)0)]
        [DataRow("01 00", (short)1)]
        [DataRow("FF 7F", short.MaxValue)]
        [DataRow("00 80", short.MinValue)]
        [DataRow("FE FF", (short)-2)]
        [DataRow("FF FF", (short)-1)]
        public void ReadShort_Succeeds(string input, short expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadShort();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00", 0)]
        [DataRow("01 00 00 00", 1)]
        [DataRow("FF FF FF 7F", int.MaxValue)]
        [DataRow("00 00 00 80", int.MinValue)]
        [DataRow("FE FF FF FF", -2)]
        [DataRow("FF FF FF FF", -1)]
        public void ReadInt_Succeeds(string input, int expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadInt();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00 00 00 00 00", (long)0)]
        [DataRow("01 00 00 00 00 00 00 00", (long)1)]
        [DataRow("FF FF FF FF FF FF FF 7F", long.MaxValue)]
        [DataRow("00 00 00 00 00 00 00 80", long.MinValue)]
        [DataRow("FE FF FF FF FF FF FF FF", (long)-2)]
        [DataRow("FF FF FF FF FF FF FF FF", (long)-1)]
        public void ReadLong_Succeeds(string input, long expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadLong();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }


        [TestMethod]
        [DataRow("00 00", (ushort)0)]
        [DataRow("01 00", (ushort)1)]
        [DataRow("FF 7F", (ushort)short.MaxValue)]
        [DataRow("00 80", (ushort)((ushort)short.MaxValue + 1))]
        [DataRow("FE FF", (ushort)(ushort.MaxValue - 1))]
        [DataRow("FF FF", ushort.MaxValue)]
        public void ReadUShort_Succeeds(string input, ushort expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadUShort();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00", (uint)0)]
        [DataRow("01 00 00 00", (uint)1)]
        [DataRow("FF FF FF 7F", (uint)int.MaxValue)]
        [DataRow("00 00 00 80", (uint)int.MaxValue + 1)]
        [DataRow("FE FF FF FF", uint.MaxValue - 1)]
        [DataRow("FF FF FF FF", uint.MaxValue)]
        public void ReadUInt_Succeeds(string input, uint expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadUInt();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00 00 00 00 00", (ulong)0)]
        [DataRow("01 00 00 00 00 00 00 00", (ulong)1)]
        [DataRow("FF FF FF FF FF FF FF 7F", (ulong)long.MaxValue)]
        [DataRow("00 00 00 00 00 00 00 80", (ulong)long.MaxValue + 1)]
        [DataRow("FE FF FF FF FF FF FF FF", ulong.MaxValue - 1)]
        [DataRow("FF FF FF FF FF FF FF FF", ulong.MaxValue)]
        public void ReadULong_Succeeds(string input, ulong expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadULong();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00", false)]
        [DataRow("01", true)]
        [DataRow("FF", true)]
        public void ReadBool_Succeeds(string input, bool expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadBool();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 01", 2, "00 01", 0)]
        [DataRow("00 01", 100, "00 01", 0)]
        [DataRow("00 01 02 03", 3, "00 01 02", 1)]
        public void ReadBytes_Succeeds(string input, int length, string expected, int expectedAvailable)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadBytes(length);
                Assert.AreEqual(expected, result.ByteArrayToString());
                Assert.AreEqual(expectedAvailable, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBytes_Negative_Fails()
        {
            var packet = new PacketReader(Functions.HexToBytes("01 02 03 04"));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => packet.ReadBytes(-1));
        }

        [TestMethod]
        public void ReadPoint_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01 00 02 00")))
            {
                var result = packet.ReadPoint();
                Assert.AreEqual(1, result.X);
                Assert.AreEqual(2, result.Y);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("01", ClientOperationCode.Login)]
        [DataRow("09", ClientOperationCode.Pong)]
        [DataRow("FF", ClientOperationCode.Unknown)]
        public void ReadHeader_Valid_Succeeds(string input, ClientOperationCode expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadHeader();
                Assert.AreEqual((byte)expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("05 00 48 65 6C 6C 6F", "Hello")]
        public void ReadString_Valid_Succeeds(string input, string expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadString();
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("0A 00 48 65 6C 6C 6F")]
        [DataRow("FF 00 48 65 6C 6C 6F")]
        public void ReadString_Invalid_Fails(string input)
        {
            var packet = new PacketReader(Functions.HexToBytes(input));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => packet.ReadString());
        }

        [TestMethod]
        [DataRow("48 65 6C 6C 6F 00 00 00 00 00", 10, "Hello")]
        public void ReadString_WithLength_Succeeds(string input, int length, string expected)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                var result = packet.ReadString(length);
                Assert.AreEqual(expected, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("48 65 6C 6C 6F", -1)]
        [DataRow("48 65 6C 6C 6F", 100)]
        public void ReadString_WithInvalidLength_Fails(string input, int length)
        {
            var packet = new PacketReader(Functions.HexToBytes(input));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => packet.ReadString(length));
        }

        [TestMethod]
        [DataRow("00 00 00 00", 0, 4)]
        [DataRow("00 00 00 00", 1, 3)]
        [DataRow("00 00 00 00", 2, 2)]
        [DataRow("00 00 00 00", 4, 0)]
        public void Skip_Valid_Succeeds(string input, int skip, int expectedAvailable)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                packet.Skip(skip);
                Assert.AreEqual(expectedAvailable, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00", 100)]
        [DataRow("00 00 00 00", -1)]
        public void Skip_Invalid_Fails(string input, int skip)
        {
            var packet = new PacketReader(Functions.HexToBytes(input));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => packet.Skip(skip));
        }

        [TestMethod]
        [DataRow("00 00 00 00", 3, 1, 3)]
        [DataRow("00 00 00 00", 4, 2, 2)]
        [DataRow("00 00 00 00", 2, 3, 1)]
        [DataRow("00 00 00 00", 2, 4, 0)]
        public void Seek_Succeeds(string input, int skip, int seek, int expectedAvailable)
        {
            using (var packet = new PacketReader(Functions.HexToBytes(input)))
            {
                packet.Skip(skip);
                packet.Seek(seek);
                Assert.AreEqual(expectedAvailable, packet.Available);
            }
        }

        [TestMethod]
        [DataRow("00 00 00 00", -1)]
        [DataRow("00 00 00 00", 5)]
        [DataRow("00 00 00 00", 100)]
        public void Seek_Invalid_Fails(string input, int skip)
        {
            var packet = new PacketReader(Functions.HexToBytes(input));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => packet.Seek(skip));
        }
    }
}
