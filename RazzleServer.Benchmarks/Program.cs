using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RazzleServer.Common.Crypto;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Benchmarks
{
    public static class Program
    {
        public static void Main() => BenchmarkRunner.Run<Test>(new Config());
    }

    public class Test {

        private MapleCipher cipher = new MapleCipher((ushort)55, (ulong)0x52330F1BB4060813);
        private byte[] data;

        public Test()
        {
            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            data = packet.ToArray();
        }

        [Benchmark]
        public byte[] Encrypt() {
            cipher.SetIv(0);
            var encryptedPacket = cipher.Encrypt(data, false);
            return encryptedPacket.ToArray();
        } 

        [Benchmark]
        public byte[] EncryptDecrypt() {
            cipher.SetIv(0);
            var encryptedPacket = cipher.Encrypt(data, false);
            cipher.SetIv(0);
            var decryptedPacket = cipher.Decrypt(encryptedPacket);
            return decryptedPacket.ToArray();
        } 
        
    }
}
