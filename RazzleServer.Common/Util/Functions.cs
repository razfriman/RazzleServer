using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Serilog;

namespace RazzleServer.Common.Util
{
    public class Functions
    {
        private static readonly ILogger Logger = Log.ForContext<Functions>();

        /// <summary>
        /// Global random against time-based seed mistakes
        /// </summary>
        private static readonly Random Rand = new Random();

        private static readonly char[] AsciiChars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        /// <summary>
        /// Checks whether a string contains only alpha numerical characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsAlphaNumerical(string s) =>
            !string.IsNullOrEmpty(s) && new Regex("^[a-zA-Z0-9]*$").IsMatch(s);

        public static byte[] AsciiToBytes(string s) => Encoding.ASCII.GetBytes(s);

        private static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            if (val < 38 || val > 57 && val < 65 || val > 70)
            {
                return -1; //NOT a hex value.
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
                return null; //odd number of hex digits.
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

            return Rand.Next(0, 100) < chance;
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

            return Rand.NextDouble() * 100 < chance;
        }

        /// <summary>
        /// Creates a random double between 0.0 and 0.1
        /// </summary>
        public static double RandomDouble() => Rand.NextDouble();

        /// <summary>
        /// Creates a random byte
        /// </summary>
        public static byte RandomByte() => RandomBytes(1)[0];

        /// <summary>
        /// Creates a random array of bytes
        /// </summary>
        public static byte[] RandomBytes(int length)
        {
            var randomBytes = new byte[length];
            Rand.NextBytes(randomBytes);
            return randomBytes;
        }

        /// <summary>
        /// Creates a boolean that is randomly true or false
        /// </summary>
        /// <returns></returns>
        public static bool RandomBoolean() => Rand.Next(0, 100) < 50;

        public static uint RandomUInt() => BitConverter.ToUInt32(RandomBytes(4), 0);

        public static long RandomLong() => BitConverter.ToInt64(RandomBytes(8), 0);

        /// <summary>
        /// Creates a random int
        /// </summary>
        public static int Random() => (int)Math.Floor((double)Rand.Next());

        /// <summary>
        /// Creates a random with an exclusive upper bound
        /// </summary>
        public static int Random(int max) => Rand.Next(max);

        /// <summary>
        /// Creates a random int with an inclusive min and inclusive max value
        /// </summary>
        /// <param name="min">Lowest value in range</param>
        /// <param name="max">Highest value in range</param>
        public static int Random(int min, int max) => Rand.Next(min, max + 1);

        public static string RandomString(int length = 20)
        {
            var data = new byte[1];
            using var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[length];
            crypto.GetNonZeroBytes(data);

            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(AsciiChars[b % AsciiChars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Creates a SHA1 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <returns>The SHA1 equivalent of value</returns>
        public static string GetSha512(string value)
        {
            var data = Encoding.ASCII.GetBytes(value);

            var sha = SHA512.Create();
            var hashData = sha.ComputeHash(data);

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
        public static string GetHmacSha512(string value, string key) =>
            GetHmacSha512(value, Encoding.ASCII.GetBytes(key));

        /// <summary>
        /// Creates a HMACSHA512 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <param name="key">Key used for the hash</param>
        /// <returns>The HMACSHA512 equivalent of value</returns>
        public static string GetHmacSha512(string value, byte[] key)
        {
            var data = Encoding.ASCII.GetBytes(value);
            using var sha = new HMACSHA512(key);
            var hashData = sha.ComputeHash(data);

            var hash = new StringBuilder();

            foreach (var b in hashData)
            {
                hash.Append(b.ToString("X2"));
            }

            return hash.ToString();
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

        public static void SaveToJson<T>(string path, T data) where T : class
        {
            using var s = File.OpenWrite(path);
            SaveToJson(s, data);
        }

        public static void SaveToJson<T>(Stream stream, T data) where T : class
        {
            using var sw = new StreamWriter(stream);
            using var writer = new JsonTextWriter(sw);
            try
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while saving to JSON");
            }
        }
    }
}
