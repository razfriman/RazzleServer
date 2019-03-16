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
                file.Export(ms);
                var contents = Encoding.ASCII.GetString(ms.ToArray());
                Assert.AreEqual(374, contents.Length);
            }
        }

        [TestMethod]
        public void TestWzFile1()
        {
            var img = new WzImage("test.img");
            img.AddProperty(new WzIntProperty("int1", 100));
            var file = new WzFile(1, WzMapleVersionType.Classic);
            file.WzDirectory.WzImages.Add(img);

            using (var ms = new MemoryStream())
            {
                file.Export(ms);
                var contents = Encoding.ASCII.GetString(ms.ToArray());
                Assert.AreEqual(672, contents.Length);
            }
        }
    }
}