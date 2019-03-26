using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Tests
{
    [TestClass]
    public class PacketWriterTests
    {
        [TestMethod]
        public void EmptyPacketWriter_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                Assert.AreEqual("", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteByte_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteByte(1);
                Assert.AreEqual("01", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteShort_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteShort(2);
                Assert.AreEqual("02 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteInt_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteInt(4);
                Assert.AreEqual("04 00 00 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteLong_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteLong(8);
                Assert.AreEqual("08 00 00 00 00 00 00 00", pw.ToPacketString());
            }
        }


        [TestMethod]
        public void WriteUShort_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteUShort(ushort.MaxValue);
                Assert.AreEqual("FF FF", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteUInt_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteUInt(uint.MaxValue);
                Assert.AreEqual("FF FF FF FF", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteULong_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteULong(ulong.MaxValue);
                Assert.AreEqual("FF FF FF FF FF FF FF FF", pw.ToPacketString());
            }
        }


        [TestMethod]
        public void WriteBox_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBox(new Common.Util.Rectangle(new Common.Util.Point(1, 2), new Common.Util.Point(3, 4)));
                Assert.AreEqual("01 00 00 00 02 00 00 00 03 00 00 00 04 00 00 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteBool_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBool(true);
                Assert.AreEqual("01", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteBytes_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteBytes(new byte[] { 1, 2, 3, 4 });
                Assert.AreEqual("01 02 03 04", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WritePoint_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WritePoint(new Common.Util.Point(1, 2));
                Assert.AreEqual("01 00 02 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteHeader_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteHeader(ServerOperationCode.Fame);
                Assert.AreEqual("19", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteString_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteString("Hello");
                Assert.AreEqual("05 00 48 65 6C 6C 6F", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteString_WithLength_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteString("Hello", 10);
                Assert.AreEqual("48 65 6C 6C 6F 00 00 00 00 00", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void WriteDateTime_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteDateTime(new DateTime(2000, 12, 25));
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
        public void WriteHexString_Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteHexString("01 02 03 04");
                Assert.AreEqual("01 02 03 04", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void _Valid_Succeeds()
        {
            using (var pw = new PacketWriter())
            {
                pw.WriteZeroBytes(4);
                Assert.AreEqual("00 00 00 00", pw.ToPacketString());
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
        public void Constructor_FromServerOperationCode_Succeeds()
        {
            using (var pw = new PacketWriter(ServerOperationCode.Fame))
            {
                Assert.AreEqual("19", pw.ToPacketString());
            }
        }

        [TestMethod]
        public void Constructor_FromByte_Succeeds()
        {
            using (var pw = new PacketWriter(1))
            {
                Assert.AreEqual("01", pw.ToPacketString());
            }
        }


        [TestMethod]
        public void Constructor_FromBytes_Succeeds()
        {
            using (var pw = new PacketWriter(new byte[] { 1, 2, 3, 4 }))
            {
                Assert.AreEqual("01 02 03 04", pw.ToPacketString());
            }
        }
    }
}
