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

        private readonly MapleCipher _encryptor = new MapleCipher(55, 0x52330F1BB4060813);
        private readonly MapleCipher _decryptor = new MapleCipher(55, 0x52330F1BB4060813);
        private readonly byte[] _data;
        private readonly int _n = 10;
        
        public Test()
        {
            var packet = new PacketWriter();
            packet.WriteByte(1);
            packet.WriteShort(2);
            packet.WriteInt(4);
            packet.WriteLong(8);
            _data = packet.ToArray();
            _encryptor.SetIv(0);
            _decryptor.SetIv(0);
        }

        [Benchmark]
        public Span<byte> Encrypt() {
            var encryptedPacket = _encryptor.Encrypt(_data, false);

            for (var i = 0; i < _n; i++)
            {
                encryptedPacket = _encryptor.Encrypt(_data, false);
            }

            return encryptedPacket;
        } 

        [Benchmark]
        public Span<byte> EncryptDecrypt() {
            var encryptedPacket = _encryptor.Encrypt(_data, false);
            var decryptedPacket = _decryptor.Decrypt(encryptedPacket);

            for (var i = 0; i < _n; i++)
            {
                encryptedPacket = _encryptor.Encrypt(_data, false);
                decryptedPacket = _decryptor.Decrypt(encryptedPacket);
            }

            return decryptedPacket;
        } 
        
    }
}
