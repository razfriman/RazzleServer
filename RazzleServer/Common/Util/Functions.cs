using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RazzleServer.Common;

namespace RazzleServer.Common.Util
{
    public static class Functions
    {
        /// <summary>
        /// Global random against time-based seed mistakes
        /// </summary>
        private static readonly Random R = new Random();

        /// <summary>
        /// Checks whether a string contains only alpha numerical characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsAlphaNumerical(string s) => !string.IsNullOrEmpty(s) && new Regex("^[a-zA-Z0-9]*$").IsMatch(s);

        public static byte[] AsciiToBytes(string s) => Encoding.ASCII.GetBytes(s);

        public static string ByteArrayToString(this Memory<byte> bArray, bool endingSpace = true, bool nospace = false)
        {
            return ByteArrayToString(bArray.ToArray(), endingSpace, nospace);
        }

        public static string ByteArrayToString(this Span<byte> bArray, bool endingSpace = true, bool nospace = false)
        {
            return ByteArrayToString(bArray.ToArray(), endingSpace, nospace);
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="bArray"></param>
        /// <param name="endingSpace"></param>
        /// <param name="nospace"></param>
        /// <returns></returns>
        public static string ByteArrayToString(this byte[] bArray, bool endingSpace = true, bool nospace = false)
        {
            //this function is literally the most beautiful thing you've ever seen
            //admit it.
            byte multi = 3;
            if (nospace)
            {
                multi = 2;
            }

            var ret = new char[bArray.Length * multi];
            var bytearraycounter = 0;
            for (var i = 0; i < ret.Length; i += multi)
            {
                var b = bArray[bytearraycounter++];
                var b2 = (byte)((b & 0x0F) + 6);
                b = (byte)((b >> 4) + 6);
                ret[i] = (char)(42 + b + 7 * (b >> 4));
                ret[i + 1] = (char)(42 + b2 + 7 * (b2 >> 4));
                if (!nospace)
                {
                    ret[i + 2] = ' ';
                }
            }
            var length = ret.Length;
            if (!endingSpace && length != 0)
            {
                length--;
            }

            return new string(ret, 0, length).Trim();
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            if (val < 38 || val > 57 && val < 65 || val > 70)
            {
                return -1;//NOT a hex value.
            }

            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="hex">A hex string like "FF 00"</param>
        /// <returns>null if the hex string is invalid.</returns>
        public static byte[] HexToBytes(string hex)
        {
            hex = hex.Replace(" ", "").ToUpper();
            if (hex.Length % 2 == 1)
            {
                return null;//odd number of hex digits.
            }

            var arr = new byte[hex.Length >> 1];

            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                var v1 = GetHexVal(hex[i << 1]);
                var v2 = GetHexVal(hex[(i << 1) + 1]);
                if (v1 == -1 || v2 == -1)
                {
                    return null;
                }

                arr[i] = (byte)((v1 << 4) + v2);
            }
            return arr;
        }

        /// <summary>
        /// Returns a random boolean using a percentage for the return value to be 'true'
        /// </summary>
        /// <param name="chance">The 0-100 ranging chance to return a 'true'</param>
        public static bool MakeChance(int chance)
        {
            if (chance <= 0)
            {
                return false;
            }

            if (chance >= 100)
            {
                return true;
            }

            return R.Next(0, 100) < chance;
        }

        public static bool MakeChance(double chance)
        {
            if (chance <= 0)
            {
                return false;
            }

            if (chance >= 100)
            {
                return true;
            }

            return R.NextDouble() * 100 < chance;
        }

        /// <summary>
        /// Creates a random double between 0.0 and 0.1
        /// </summary>
        public static double RandomDouble() => R.NextDouble();

        /// <summary>
        /// Creates a random byte
        /// </summary>
        public static byte RandomByte() => (byte)Math.Floor((double)(R.Next() / 0x1010101));

        /// <summary>
        /// Creates a random array of bytes
        /// </summary>
        public static byte[] RandomBytes(int length)
        {
            var randomBytes = new byte[length];
            R.NextBytes(randomBytes);
            return randomBytes;
        }

        /// <summary>
        /// Creates a boolean that is randomly true or false
        /// </summary>
        /// <returns></returns>
        public static bool RandomBoolean() => R.Next(0, 100) < 50;

        public static uint RandomUInt() => BitConverter.ToUInt32(RandomBytes(4), 0);

        public static long RandomLong() => BitConverter.ToInt64(RandomBytes(8), 0);

        /// <summary>
        /// Creates a random int
        /// </summary>
        public static int Random() => (int)Math.Floor((double)R.Next());

        /// <summary>
        /// Creates a random with an exclusive upper bound
        /// </summary>
        public static int Random(int max) => R.Next(max);

        /// <summary>
        /// Creates a random int with an inclusive min and inclusive max value
        /// </summary>
        /// <param name="min">Lowest value in range</param>
        /// <param name="max">Highest value in range</param>
        public static int Random(int min, int max) => R.Next(min, max + 1);

        public static string RandomString(int length = 20)
        {
            var chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            var data = new byte[1];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[length];
                crypto.GetNonZeroBytes(data);
            }
            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Creates a SHA1 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <returns>The SHA1 equivelant of value</returns>
        public static string GetSha512(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            byte[] hashData;

            using (var sha = SHA512.Create())
            {
                hashData = sha.ComputeHash(data);
            }

            var hash = new StringBuilder();

            foreach (var b in hashData)
            {
                hash.Append(b.ToString("X2"));
            }

            return hash.ToString();
        }

        /// <summary>
        /// Creates a HMACSHA512 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <param name="key">Key used for the hash</param>
        /// <returns>The HMACSHA512 equivalent of value</returns>
        public static string GetHmacSha512(string value, string key) => GetHmacSha512(value, Encoding.ASCII.GetBytes(key));

        /// <summary>
        /// Creates a HMACSHA512 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <param name="key">Key used for the hash</param>
        /// <returns>The HMACSHA512 equivalent of value</returns>
        public static string GetHmacSha512(string value, byte[] key)
        {
            var data = Encoding.ASCII.GetBytes(value);
            byte[] hashData;
            using (var sha = new HMACSHA512(key))
            {
                hashData = sha.ComputeHash(data);
            }

            var hash = new StringBuilder();

            foreach (var b in hashData)
            {
                hash.Append(b.ToString("X2"));
            }

            return hash.ToString();
        }

        /// <summary>
        /// Generic extension to compare 2 objects
        /// </summary>
        /// <typeparam name="T">Type to use</typeparam>
        /// <param name="inputValue">Value in</param>
        /// <param name="from">From range</param>
        /// <param name="to">To range</param>
        /// <returns></returns>
        public static bool InRange<T>(this T inputValue, T from, T to) where T : IComparable<T> => inputValue.CompareTo(from) >= 1 && inputValue.CompareTo(to) <= -1;

        /// <summary>
        /// Creates a string by combining the strings from an array with a separator (default: space) between them
        /// </summary>
        /// <param name="arr">The array to be fused</param>
        /// <param name="startIndex">The index in the array to start at</param>
        /// <param name="separator"></param>
        /// <returns>A string with all the strings from the startindex appended with a space between them</returns>
        public static string Fuse(this string[] arr, int startIndex = 0, int? length = null, string separator = " ")
        {
            var ret = new StringBuilder();
            var loopLength = length ?? arr.Length;

            for (var i = startIndex; i < loopLength; i++)
            {
                ret.Append(arr[i]);
                if (i != arr.Length - 1)
                {
                    ret.Append(separator);
                }
            }

            return ret.ToString();
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var len = list.Count;
            for (var i = len - 1; i >= 1; --i)
            {
                var j = R.Next(i);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static void SpanCopy<T>(Span<T> src, int srcOffset, Span<T> dest, int destOffset, int length)
        {
            var srcSlice = src.Slice(srcOffset, length);
            var dstSlice = dest.Slice(destOffset, length);
            srcSlice.CopyTo(dstSlice);
        }

        public static void MemoryCopy<T>(Memory<T> src, int srcOffset, Memory<T> dest, int destOffset, int length)
        {
            var srcSlice = src.Slice(srcOffset, length);
            var dstSlice = dest.Slice(destOffset, length);
            srcSlice.CopyTo(dstSlice);
        }
    }
}
