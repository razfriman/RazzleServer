using System;
using System.Linq;

namespace RazzleServer.Common.Crypto
{
    /// <summary>
    /// Initialization vector used by the Cipher class
    /// </summary>
    public class InitializationVector
    {
        /// <summary>
        /// IV Container
        /// </summary>
        private uint _value;

        /// <summary>
        /// Gets the bytes of the current container
        /// </summary>
        internal byte[] Bytes => BitConverter.GetBytes(_value);

        /// <summary>
        /// Gets the HIWORD from the current container
        /// </summary>
        internal ushort Hiword => (ushort)(_value >> 16);

        /// <summary>
        /// Gets the LOWORD from the current container
        /// </summary>
        internal ushort Loword => (ushort)_value;

        /// <summary>
        /// IV Security check
        /// </summary>
        internal bool MustSend => Loword % 0x1F == 0;

        /// <summary>
        /// Creates a IV instance using <paramref name="vector"/>
        /// </summary>
        /// <param name="vector">Initialization vector</param>
        internal InitializationVector(uint vector) => _value = vector;

        /// <summary>
        /// Shuffles the current IV to the next vector using the shuffle table
        /// </summary>
        internal void Shuffle()
        {
            for (var i = 0; i < 4; i++)
            {
                var newIv = CryptoConstants.DefaultKey.ToArray();
                var input = (byte)(_value & 0xFF);
                var tableInput = CryptoConstants.Shuffle[input];
                newIv[0] += (byte)(CryptoConstants.Shuffle[newIv[1]] - input);
                newIv[1] -= (byte)(newIv[2] ^ tableInput);
                newIv[2] ^= (byte)(CryptoConstants.Shuffle[newIv[3]] + input);
                newIv[3] -= (byte)(newIv[0] - tableInput);

                var val = BitConverter.ToUInt32(newIv, 0);
                var val2 = val >> 0x1D;
                val <<= 0x03;
                val2 |= val;
                _value = val2;
            }
        }
    }
}
