using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Wz;
using RazzleServer.Wz.WzProperties;
using WzFile = RazzleServer.Wz.WzFile;
using WzImage = RazzleServer.Wz.WzImage;
using WzIntProperty = RazzleServer.Wz.WzProperties.WzIntProperty;
using WzObject = RazzleServer.Wz.WzObject;

namespace RazzleServer.Tests
{
    [TestClass]
    public class WzFileTests
    {
        [TestMethod]
        public void TestEmptyWzFileSerializes()
        {
            var file = new WzFile(1, WzMapleVersionType.Classic);

            using var ms = new MemoryStream();
            file.Serialize(ms);
            var contents = Encoding.ASCII.GetString(ms.ToArray());
            var deserialized = WzObject.DeserializeFile(contents);
            Assert.IsNotNull(deserialized);
        }

        [TestMethod]
        public void TestWzFileWithImageAndPropertySerializes()
        {
            var intProp = new WzIntProperty("int1", 100);
            var img = new WzImage("test.img");
            var file = new WzFile(1, WzMapleVersionType.Classic);
            img.AddProperty(intProp);
            file.WzDirectory.WzImages.Add(img);

            using var ms = new MemoryStream();
            file.Serialize(ms);
            var contents = Encoding.ASCII.GetString(ms.ToArray());
            var deserialized = WzObject.DeserializeFile(contents);
            var deserializedImg = deserialized.WzDirectory.GetImageByName(img.Name);
            Assert.IsNotNull(deserializedImg);
            Assert.AreEqual(intProp.Value, deserializedImg[intProp.Name].GetInt());
        }

        [TestMethod]
        public void TestEmptyWzFileSerializesProto()
        {
            var file = new WzFile(1, WzMapleVersionType.Classic);

            using var ms = new MemoryStream();
            file.Serialize(ms);
            var contents = Encoding.ASCII.GetString(ms.ToArray());
            var deserialized = WzObject.DeserializeFile(contents);
            Assert.IsNotNull(deserialized);
        }

        [TestMethod]
        public void TestWzFileWithImageAndPropertySerializesProtoAndJson()
        {
            var intProp = new WzIntProperty("int1", 100);
            var img = new WzImage("test.img");
            var file = new WzFile(1, WzMapleVersionType.Classic);
            img.AddProperty(intProp);
            file.WzDirectory.WzImages.Add(img);

            // Serialize as Protobuf then JSON
            using var ms = new MemoryStream();
            file.SerializeProto(ms);
            ms.Position = 0;
            var deserializedProtoBuf = WzObject.DeserializeProtoFile(ms);
            using var msProtobuf = new MemoryStream();
            deserializedProtoBuf.Serialize(msProtobuf);
            var contentsProtobuf = Encoding.ASCII.GetString(msProtobuf.ToArray());

            // Serialize as JSON
            using var msJson = new MemoryStream();
            file.Serialize(msJson);
            var contentsJson = Encoding.ASCII.GetString(msJson.ToArray());

            Console.WriteLine(contentsJson);

            Console.WriteLine();

            Console.WriteLine(contentsProtobuf);

            Assert.AreEqual(contentsJson, contentsProtobuf);
        }

        [TestMethod]
        public void TestWzFileWithImageAndPropertySerializesProto()
        {
            var subProperty = new WzSubProperty("sub1");
            var subIntProperty = new WzIntProperty("int2", 200);
            var intProp = new WzIntProperty("int1", 100);
            var img = new WzImage("test.img");
            var file = new WzFile(1, WzMapleVersionType.Classic);
            subProperty.AddProperty(subIntProperty);
            img.AddProperty(intProp);
            img.AddProperty(subProperty);
            file.WzDirectory.WzImages.Add(img);

            using var ms = new MemoryStream();
            file.SerializeProto(ms);
            ms.Position = 0;
            var deserialized = WzObject.DeserializeProtoFile(ms);

            var deserializedImg = deserialized.WzDirectory.GetImageByName(img.Name);
            Assert.IsNotNull(deserializedImg);

            Assert.AreEqual(intProp.Value, deserializedImg[intProp.Name].GetInt());
            Assert.AreEqual(subIntProperty.Value, deserializedImg[subProperty.Name][subIntProperty.Name].GetInt());
        }
    }
}
