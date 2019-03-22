using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Wz;
using RazzleServer.Common.Wz.WzProperties;

namespace RazzleServer.Tests
{
    [TestClass]
    public class WzFileTests
    {
        [TestMethod]
        public void TestWzFile()
        {
            var file = new WzFile(1, WzMapleVersionType.Classic);

            using (var ms = new MemoryStream())
            {
                file.Serialize(ms);
                var contents = Encoding.ASCII.GetString(ms.ToArray());
                var deserialized = WzObject.DeserializeFile(contents);
                Assert.IsNotNull(deserialized);
            }
        }

        [TestMethod]
        public void TestWzFile1()
        {
            var intProp = new WzIntProperty("int1", 100);
            var img = new WzImage("test.img");
            var file = new WzFile(1, WzMapleVersionType.Classic);
            img.AddProperty(intProp);
            file.WzDirectory.WzImages.Add(img);

            using (var ms = new MemoryStream())
            {
                file.Serialize(ms);
                var contents = Encoding.ASCII.GetString(ms.ToArray());
                var deserialized = WzObject.DeserializeFile(contents);
                var deserializedImg = deserialized[img.Name];
                Assert.IsNotNull(deserializedImg);
                Assert.AreEqual(intProp.Value, deserializedImg[intProp.Name].GetInt());
            }
        }
    }
}
