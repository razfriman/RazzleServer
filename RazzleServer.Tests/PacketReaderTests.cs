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
        public void EmptyPacketReader_Valid_Succeeds()
        {
            using (var packet = new PacketReader(new byte[] { }))
            {
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadByte_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01")))
            {
                var result = packet.ReadByte();
                Assert.AreEqual(1, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadSByte_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("FF")))
            {
                var result = packet.ReadSByte();
                Assert.AreEqual(-1, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadShort_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("02 00")))
            {
                var result = packet.ReadShort();
                Assert.AreEqual(2, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadInt_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("04 00 00 00")))
            {
                var result = packet.ReadInt();
                Assert.AreEqual(4, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadLong_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("08 00 00 00 00 00 00 00")))
            {
                var result = packet.ReadLong();
                Assert.AreEqual(8, result);
                Assert.AreEqual(0, packet.Available);
            }
        }


        [TestMethod]
        public void ReadUShort_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("FF FF")))
            {
                var result = packet.ReadUShort();
                Assert.AreEqual(ushort.MaxValue, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadUInt_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("FF FF FF FF")))
            {
                var result = packet.ReadUInt();
                Assert.AreEqual(uint.MaxValue, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadULong_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("FF FF FF FF FF FF FF FF")))
            {
                var result = packet.ReadULong();
                Assert.AreEqual(ulong.MaxValue, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBool_True_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01")))
            {
                var result = packet.ReadBool();
                Assert.AreEqual(true, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBool_False_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("00")))
            {
                var result = packet.ReadBool();
                Assert.AreEqual(false, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBool_Invalid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("FF")))
            {
                var result = packet.ReadBool();
                Assert.AreEqual(true, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBytes_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01 02 03 04")))
            {
                var result = packet.ReadBytes(4);
                Assert.AreEqual("01 02 03 04", result.ByteArrayToString());
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadBytes_Invalid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01 02 03 04")))
            {
                var result = packet.ReadBytes(10);
                Assert.AreEqual("01 02 03 04", result.ByteArrayToString());
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadBytes_Negative_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01 02 03 04")))
            {
                packet.ReadBytes(-1);
            }
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
        public void ReadHeader_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("01")))
            {
                var result = packet.ReadHeader();
                Assert.AreEqual((ushort)ClientOperationCode.Login, result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadString_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("05 00 48 65 6C 6C 6F")))
            {
                var result = packet.ReadString();
                Assert.AreEqual("Hello", result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void ReadString_WithLength_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("48 65 6C 6C 6F 00 00 00 00 00")))
            {
                var result = packet.ReadString(10);
                Assert.AreEqual("Hello", result);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void Skip_Valid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("00 00 00 00")))
            {
                packet.Skip(4);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        public void Skip_Invalid_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("00 00 00 00")))
            {
                packet.Skip(10);
                Assert.AreEqual(0, packet.Available);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Skip_Negative_Succeeds()
        {
            using (var packet = new PacketReader(Functions.HexToBytes("00 00 00 00")))
            {
                packet.Skip(-1);
            }
        }
    }
}
