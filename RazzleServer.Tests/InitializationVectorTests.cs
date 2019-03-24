using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Util;

namespace RazzleServer.Tests
{
    [TestClass]
    public class InitializationVectorTests
    {
        [TestMethod]
        public void MaxValueIvCastsSuccessfully()
        {
            var iv = new InitializationVector(uint.MaxValue);
            Assert.AreEqual(ushort.MaxValue, iv.HiWord);
            Assert.AreEqual(ushort.MaxValue, iv.LoWord);
            CollectionAssert.AreEqual(new byte[]{0xFF, 0xFF, 0xFF, 0xFF}, iv.Bytes);
            Assert.AreEqual(uint.MaxValue, iv.UInt);
        }
        
        [TestMethod]
        public void ZeroValueIvCastsSuccessfully()
        {
            var iv = new InitializationVector(0);
            Assert.AreEqual(0, iv.HiWord);
            Assert.AreEqual(0, iv.LoWord);
            CollectionAssert.AreEqual(new byte[4], iv.Bytes);
            Assert.AreEqual(0u, iv.UInt);
        }
        
        [TestMethod]
        public void ShuffleProvidesExpectedResult()
        {
            var iv = new InitializationVector(0);
            CollectionAssert.AreEqual(new byte[4], iv.Bytes);
            iv.Shuffle();
            CollectionAssert.AreEqual(new byte[]{ 0x11, 0xBB, 0x64, 0xC7}, iv.Bytes);
        }
    }
}
