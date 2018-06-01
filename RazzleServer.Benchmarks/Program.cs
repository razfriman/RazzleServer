using System;
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

        private MapleCipher encryptor = new MapleCipher((ushort)55, (ulong)0x52330F1BB4060813);
        private MapleCipher decryptor = new MapleCipher((ushort)55, (ulong)0x52330F1BB4060813);
        private byte[] data;
        private int N = 10;
        public Test()
        {
            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            data = packet.ToArray();
            encryptor.SetIv(0);
            decryptor.SetIv(0);
        }

        [Benchmark]
        public Span<byte> Encrypt() {
            var encryptedPacket = encryptor.Encrypt(data, false);

            for (int i = 0; i < N; i++)
            {
                encryptedPacket = encryptor.Encrypt(data, false);
            }

            return encryptedPacket;
        } 

        [Benchmark]
        public Span<byte> EncryptDecrypt() {
            var encryptedPacket = encryptor.Encrypt(data, false);
            var decryptedPacket = decryptor.Decrypt(encryptedPacket);

            for (int i = 0; i < N; i++)
            {
                encryptedPacket = encryptor.Encrypt(data, false);
                decryptedPacket = decryptor.Decrypt(encryptedPacket);
            }

            return decryptedPacket;
        } 
        
    }
}
