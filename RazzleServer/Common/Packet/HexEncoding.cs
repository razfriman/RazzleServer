using System;

namespace RazzleServer.Common.Packet
{
    /// <summary>
    /// Class to handle Hex Encoding and Hex Conversions
    /// </summary>
    public static class HexEncoding
    {

        /// <summary>
        /// Checks if a character is a hex digit
        /// </summary>
        /// <param name="pChar">Char to check</param>
        /// <returns>Char is a hex digit</returns>
        public static bool IsHexDigit(Char pChar)
        {
            int numChar;
            var numA = Convert.ToInt32('A');
            var num1 = Convert.ToInt32('0');
            pChar = Char.ToUpper(pChar);
            numChar = Convert.ToInt32(pChar);

            if (numChar >= numA && numChar < (numA + 6))
            {
                return true;
            }

            if (numChar >= num1 && numChar < (num1 + 10))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert a hex string to a byte
        /// </summary>
        /// <param name="pHex">Byte as a hex string</param>
        /// <returns>Byte representation of the string</returns>
        private static byte HexToByte(string pHex)
        {
            if (pHex.Length > 2 || pHex.Length <= 0)
            {
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            }

            var newByte = byte.Parse(pHex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

		/// <summary>
		/// Convert a hex string to a byte array
		/// </summary>
		/// <param name="hexString">byte array as a hex string</param>
		/// <returns>Byte array representation of the string</returns>
        public static byte[] GetBytes(string hexString)
        {
            var newString = string.Empty;
            char c;

            // remove all none A-F, 0-9, characters
            for (var i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                {
                    newString += c;
                }
            }

            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                newString = newString.Substring(0, newString.Length - 1);
            }

            var byteLength = newString.Length / 2;
            var bytes = new byte[byteLength];
            string hex;
            var j = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }

            return bytes;
        }
    }
}