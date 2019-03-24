using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace RazzleServer.Common.Crypto
{
    /// <summary>
    /// Initialization vector used by the Cipher class
    /// </summary>
    public class InitializationVector
    {
        private readonly byte[] _data;

        /// <summary>
        /// Gets the uint value from the current container
        /// </summary>
        public uint UInt => MemoryMarshal.Cast<byte, uint>(_data)[0];
        /// <summary>
        /// Gets the LOWORD from the current container
        /// </summary>
        public ushort LoWord => MemoryMarshal.Cast<byte, ushort>(_data)[0];
        
        /// <summary>
        /// Gets the HIWORD from the current container
        /// </summary>
        public ushort HiWord => MemoryMarshal.Cast<byte, ushort>(_data)[1];

        /// <summary>
        /// Gets the bytes of the current container
        /// </summary>
        public byte[] Bytes => _data.ToArray();
        
        /// <summary>
        /// IV Security check
        /// </summary>
        public bool MustSend => LoWord % 0x1F == 0;

        /// <summary>
        /// Creates a IV instance using <paramref name="vector"/>
        /// </summary>
        /// <param name="vector">Initialization vector</param>
        public InitializationVector(uint vector) => _data = BitConverter.GetBytes(vector);

        /// <summary>
        /// Creates a IV instance using <paramref name="vector"/>
        /// </summary>
        /// <param name="vector">Initialization vector</param>
        public InitializationVector(byte[] vector) => _data = vector;

        /// <summary>
        /// Shuffles the current IV to the next vector using the shuffle table
        /// </summary>
        public void Shuffle()
        {
            var newIv = CryptoConstants.DefaultKey.ToArray();
            
            for (var i = 0; i < 4; i++)
            {
                var input = _data[i];
                var tableInput = CryptoConstants.Shuffle[input];
                newIv[0] += (byte)(CryptoConstants.Shuffle[newIv[1]] - input);
                newIv[1] -= (byte)(newIv[2] ^ tableInput);
                newIv[2] ^= (byte)(CryptoConstants.Shuffle[newIv[3]] + input);
                newIv[3] -= (byte)(newIv[0] - tableInput);

                var val = BitConverter.ToUInt32(newIv, 0);
                var val2 = val >> 0x1D;
                val <<= 0x03;
                val2 |= val;
                newIv[0] = (byte)(val2 & 0xFF);
                newIv[1] = (byte)((val2 >> 8) & 0xFF);
                newIv[2] = (byte)((val2 >> 16) & 0xFF);
                newIv[3] = (byte)((val2 >> 24) & 0xFF);
            }
            Buffer.BlockCopy(newIv, 0, _data, 0, 4);
        }
    }
}
