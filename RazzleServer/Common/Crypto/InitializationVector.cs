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
        private uint Value;

        /// <summary>
        /// Gets the bytes of the current container
        /// </summary>
        internal byte[] Bytes => BitConverter.GetBytes(Value);

        /// <summary>
        /// Gets the HIWORD from the current container
        /// </summary>
        internal ushort HIWORD => unchecked((ushort)(Value >> 16));

        /// <summary>
        /// Gets the LOWORD from the current container
        /// </summary>
        internal ushort LOWORD => (ushort)Value;

        /// <summary>
        /// IV Security check
        /// </summary>
        internal bool MustSend => LOWORD % 0x1F == 0;

        /// <summary>
        /// Creates a IV instance using <paramref name="vector"/>
        /// </summary>
        /// <param name="vector">Initialization vector</param>
        internal InitializationVector(uint vector) => Value = vector;

        /// <summary>
        /// Shuffles the current IV to the next vector using the shuffle table
        /// </summary>
        internal unsafe void Shuffle()
        {
            var Key = CryptoConstants.DefaultKey;
            var pKey = &Key;
            fixed (uint* pIV = &Value)
            {
                fixed (byte* pShuffle = CryptoConstants.Shuffle)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        *((byte*)pKey + 0) += (byte)(*(pShuffle + *((byte*)pKey + 1)) - *((byte*)pIV + i));
                        *((byte*)pKey + 1) -= (byte)(*((byte*)pKey + 2) ^ *(pShuffle + *((byte*)pIV + i)));
                        *((byte*)pKey + 2) ^= (byte)(*((byte*)pIV + i) + *(pShuffle + *((byte*)pKey + 3)));
                        *((byte*)pKey + 3) = (byte)(*((byte*)pKey + 3) - *(byte*)pKey + *(pShuffle + *((byte*)pIV + i)));

                        *pKey = (*pKey << 3) | (*pKey >> (32 - 3));
                    }
                }
            }

            Value = Key;
        }
    }
}