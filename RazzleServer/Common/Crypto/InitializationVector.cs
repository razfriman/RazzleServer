using System;

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
        internal unsafe void Shuffle()
        {
            var key = CryptoConstants.DefaultKey;
            var pKey = &key;
            fixed (uint* pIv = &_value)
            {
                fixed (byte* pShuffle = CryptoConstants.Shuffle)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        *((byte*)pKey + 0) += (byte)(*(pShuffle + *((byte*)pKey + 1)) - *((byte*)pIv + i));
                        *((byte*)pKey + 1) -= (byte)(*((byte*)pKey + 2) ^ *(pShuffle + *((byte*)pIv + i)));
                        *((byte*)pKey + 2) ^= (byte)(*((byte*)pIv + i) + *(pShuffle + *((byte*)pKey + 3)));
                        *((byte*)pKey + 3) = (byte)(*((byte*)pKey + 3) - *(byte*)pKey + *(pShuffle + *((byte*)pIv + i)));

                        *pKey = (*pKey << 3) | (*pKey >> (32 - 3));
                    }
                }
            }

            _value = key;
        }
    }
}