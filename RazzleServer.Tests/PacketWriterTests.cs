using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Tests
{
    [TestClass]
    public class PacketWriterTests
    {
        [TestMethod]
        public void Constructor_Empty_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                Assert.AreEqual("", pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow(ServerOperationCode.CharacterList, "04")]
        [DataRow(ServerOperationCode.SetField, "26")]
        [DataRow(ServerOperationCode.Fame, "19")]
        public void Constructor_FromServerOperationCode_Succeeds(ServerOperationCode input, string expected)
        {
            using (var pw = new PacketWriter(input))
            {
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((byte)0, "00")]
        [DataRow((byte)1, "01")]
        [DataRow((byte)0xFF, "FF")]
        public void Constructor_FromByte_Succeeds(byte input, string expected)
        {
            using (var pw = new PacketWriter(input))
            {
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }


        [TestMethod]
        [DataRow(new byte[] { }, "")]
        [DataRow(new byte[] {1, 2, 3, 4}, "01 02 03 04")]
        public void Constructor_FromBytes_Succeeds(byte[] input, string expected)
        {
            using (var pw = new PacketWriter(input))
            {
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((sbyte)0, "00")]
        [DataRow((sbyte)1, "01")]
        [DataRow(sbyte.MaxValue, "7F")]
        [DataRow(sbyte.MinValue, "80")]
        [DataRow((sbyte)-2, "FE")]
        [DataRow((sbyte)-1, "FF")]
        public void WriteSByte_Succeeds(sbyte input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteSByte(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((byte)0x00, "00")]
        [DataRow((byte)0x01, "01")]
        [DataRow((byte)sbyte.MaxValue, "7F")]
        [DataRow((byte)((byte)sbyte.MaxValue + 1), "80")]
        [DataRow((byte) (byte.MaxValue - 1), "FE")]
        [DataRow(byte.MaxValue, "FF")]
        public void WriteByte_Succeeds(byte input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteByte(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((short)0, "00 00")]
        [DataRow((short)1, "01 00")]
        [DataRow(short.MaxValue, "FF 7F")]
        [DataRow(short.MinValue, "00 80")]
        [DataRow((short)-2, "FE FF")]
        [DataRow((short)-1, "FF FF")]
        public void WriteShort_Succeeds(short input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteShort(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((ushort)0x00, "00 00")]
        [DataRow((ushort)0x01, "01 00")]
        [DataRow((ushort)short.MaxValue, "FF 7F")]
        [DataRow((ushort)((ushort)short.MaxValue + 1), "00 80")]
        [DataRow((ushort)(ushort.MaxValue - 1), "FE FF")]
        [DataRow(ushort.MaxValue, "FF FF")]
        public void WriteUShort_Succeeds(ushort input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteUShort(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow(0, "00 00 00 00")]
        [DataRow(1, "01 00 00 00")]
        [DataRow(int.MaxValue, "FF FF FF 7F")]
        [DataRow(int.MinValue, "00 00 00 80")]
        [DataRow(-2, "FE FF FF FF")]
        [DataRow(-1, "FF FF FF FF")]
        public void WriteInt_Succeeds(int input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteInt(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((uint)0, "00 00 00 00")]
        [DataRow((uint)1, "01 00 00 00")]
        [DataRow((uint)int.MaxValue, "FF FF FF 7F")]
        [DataRow((uint)int.MaxValue + 1, "00 00 00 80")]
        [DataRow(uint.MaxValue - 1, "FE FF FF FF")]
        [DataRow(uint.MaxValue, "FF FF FF FF")]
        public void WriteUInt_Succeeds(uint input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteUInt(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((long)0, "00 00 00 00 00 00 00 00")]
        [DataRow((long)1, "01 00 00 00 00 00 00 00")]
        [DataRow(long.MaxValue, "FF FF FF FF FF FF FF 7F")]
        [DataRow(long.MinValue, "00 00 00 00 00 00 00 80")]
        [DataRow((long)-2, "FE FF FF FF FF FF FF FF")]
        [DataRow((long)-1, "FF FF FF FF FF FF FF FF")]
        public void WriteLong_Succeeds(long input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteLong(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow((ulong)0, "00 00 00 00 00 00 00 00")]
        [DataRow((ulong)1, "01 00 00 00 00 00 00 00")]
        [DataRow((ulong)long.MaxValue, "FF FF FF FF FF FF FF 7F")]
        [DataRow((ulong)long.MaxValue + 1, "00 00 00 00 00 00 00 80")]
        [DataRow(ulong.MaxValue - 1, "FE FF FF FF FF FF FF FF")]
        [DataRow(ulong.MaxValue, "FF FF FF FF FF FF FF FF")]
        public void WriteULong_Succeeds(ulong input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteULong(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow(false, "00")]
        [DataRow(true, "01")]
        public void WriteBool_Valid_Succeeds(bool input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBool(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow(0, "")]
        [DataRow(1, "00")]
        [DataRow(4, "00 00 00 00")]
        public void WriteZeroBytes_Succeeds(int input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteZeroBytes(4);
                Assert.AreEqual("00 00 00 00", pw.ToPacketString());
            }
        }

        [DataRow(-1)]
        [DataRow(-2)]
        public void WriteZeroBytes_InvalidArgument_Fails(int input)
        {
            var pw = new PacketWriter();
            Assert.ThrowsException<ArgumentNullException>(() => pw.WriteZeroBytes(input));
        }

        [TestMethod]
        [DataRow(new byte[] { }, "")]
        [DataRow(new byte[] {1, 2, 3, 4}, "01 02 03 04")]
        public void WriteBytes_Valid_Succeeds(byte[] input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBytes(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow("", "")]
        [DataRow("01 02 03 04", "01 02 03 04")]
        [DataRow("FF FF FF FF", "FF FF FF FF")]

        public void WriteHexString_Valid_Succeeds(string input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteHexString(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow("!@#$%^&*()")]
        [DataRow(",./;'[]<>?:{}")]
        [DataRow("ghijklmnop")]
        [DataRow("a-b-c-d-e-f-g")]
        public void WriteHexString_InvalidCharacters_Fails(string input)
        {
            var pw = new PacketWriter();
            Assert.ThrowsException<ArgumentNullException>(() => pw.WriteHexString(input));
        }

        [TestMethod]
        [DataRow(ServerOperationCode.CharacterList, "04")]
        [DataRow(ServerOperationCode.SetField, "26")]
        [DataRow(ServerOperationCode.Fame, "19")]
        public void WriteHeader_Succeeds(ServerOperationCode input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteHeader(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow("Hello", "05 00 48 65 6C 6C 6F")]
        public void WriteString_Valid_Succeeds(string input, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteString(input);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        [DataRow("Hello", 10, "48 65 6C 6C 6F 00 00 00 00 00")]
        public void WriteString_WithLength_Succeeds(string input, int inputLength, string expected)
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteString(input, inputLength);
                Assert.AreEqual(expected, pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteDateTime_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteDateTime(new DateTime(2000, 12, 25, 0, 0, 0, DateTimeKind.Utc));
                Assert.AreEqual("80 70 3E A1 E3 00 00 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteDateTime_ItemPermanentExpiration_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteDateTime(DateConstants.Permanent);
                Assert.AreEqual("80 5E B2 E1 20 03 00 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteKoreanDateTime_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteKoreanDateTime(new DateTime(2000, 12, 25));
                Assert.AreEqual("00 68 37 E5 87 6D C0 01", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WritePoint_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WritePoint(new Point(1, 2));
                Assert.AreEqual("01 00 02 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteBox_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBox(new Rectangle(new Point(1, 2), new Point(3, 4)));
                Assert.AreEqual("01 00 00 00 02 00 00 00 03 00 00 00 04 00 00 00", pw.ToPacketString());
            }
        }
    }
}
