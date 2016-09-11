using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RazzleServer.Util
{
    public static class Functions
    {
    
        /// <summary>
        /// Checks whether a string contains only alpha numerical characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsAlphaNumerical(string s)
        {
            return !string.IsNullOrEmpty(s) && new Regex("^[a-zA-Z0-9]*$").IsMatch(s);
        }

        /// <summary>
        /// Converts a byte array to a hex parsed string
        /// </summary>
        /// <param name="bytes">Byte array in</param>
        /// <returns>Hex string out</returns>
        public static string ByteArrayToStr(byte[] bytes)
        {
            return bytes.ByteArrayToString();
        }

        public static byte[] ASCIIToBytes(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="bArray"></param>
        /// <returns></returns>
        public static string ByteArrayToString(this byte[] bArray, bool endingSpace = true, bool nospace = false)
        {
            //this function is literally the most beautiful thing you've ever seen
            //admit it.
            byte multi = 3;
            if (nospace) multi = 2;
            char[] ret = new char[bArray.Length * multi];
            int bytearraycounter = 0;
            for (int i = 0; i < ret.Length; i += multi)
            {
                byte b = bArray[bytearraycounter++];
                byte b2 = (byte)((b & 0x0F) + 6);
                b = (byte)((b >> 4) + 6);
                ret[i] = (char)(42 + b + (7 * (b >> 4)));
                ret[i + 1] = (char)(42 + b2 + (7 * (b2 >> 4)));
                if (!nospace)
                    ret[i + 2] = ' ';
            }
            int length = ret.Length;
            if (!endingSpace && length != 0)
                length--;
            return new string(ret, 0, length);
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            if (val < 38 || (val > 57 && val < 65) || val > 70)
                return -1;//NOT a hex value.
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
                return null;//odd number of hex digits.
            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                int v1 = GetHexVal(hex[i << 1]);
                int v2 = GetHexVal(hex[(i << 1) + 1]);
                if (v1 == -1 || v2 == -1)
                    return null;
                arr[i] = (byte)((v1 << 4) + v2);
            }
            return arr;
        }

        /// <summary>
        /// Converts a byte array with a length of 4 to an uint
        /// </summary>
        /// <param name="bytes">Byte array of 4 in</param>
        /// <returns>Parsed uint</returns>
        public static uint ByteArrayToInt(byte[] bytes)
        {
            uint ret;
            string str = ByteArrayToStr(bytes);
            uint.TryParse(str, NumberStyles.HexNumber, null, out ret);
            return ret;
        }

        /// <summary>
        /// Global random against time-based seed mistakes
        /// </summary>
        private static readonly Random r = new Random();

        /// <summary>
        /// Returns a random boolean using a percentage for the return value to be 'true'
        /// </summary>
        /// <param name="chance">The 0-100 ranging chance to return a 'true'</param>
        public static bool MakeChance(int chance)
        {
            if (chance <= 0) return false;
            if (chance >= 100) return true;
            return r.Next(0, 100) < chance;
        }

        public static bool MakeChance(double chance)
        {
            if (chance <= 0) return false;
            if (chance >= 100) return true;
            return r.NextDouble() * 100 < chance;
        }

        /// <summary>
        /// Creates a random double between 0.0 and 0.1
        /// </summary>
        public static double RandomDouble()
        {
            return r.NextDouble();
        }

        /// <summary>
        /// Creates a random byte
        /// </summary>
        public static byte RandomByte()
        {
            return (byte)Math.Floor((double)(r.Next() / 0x1010101));
        }

        /// <summary>
        /// Creates a boolean that is randomly true or false
        /// </summary>
        /// <returns></returns>
        public static bool RandomBoolean()
        {
            return r.Next(0, 100) < 50;
        }

        public static uint RandomUInt()
        {
            byte[] randomBytes = new byte[4];
            r.NextBytes(randomBytes);
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        public static long RandomLong()
        {
            byte[] randomBytes = new byte[8];
            r.NextBytes(randomBytes);
            return BitConverter.ToInt64(randomBytes, 0);
        }

        /// <summary>
        /// Creates a random int
        /// </summary>
        public static int Random()
        {
            return (int)Math.Floor((double)r.Next());
        }

        /// <summary>
        /// Creates a random with an exclusive upper bound
        /// </summary>
        public static int Random(int max) => r.Next(max);

        public static void RandomBytes(byte[] input)
        {
            r.NextBytes(input);
        }
        /// <summary>
        /// Creates a random int with an inclusive min and inclusive max value
        /// </summary>
        /// <param name="min">Lowest value in range</param>
        /// <param name="max">Highest value in range</param>
        public static int Random(int min, int max)
        {
            return r.Next(min, max + 1);
        }

        /// <summary>
        /// Creates a SHA1 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <returns>The SHA1 equivelant of value</returns>
        public static string GetSha1(string value)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            byte[] hashData;
            
            using (var sha = SHA1.Create())
            {
                hashData = sha.ComputeHash(data);
            }

            StringBuilder hash = new StringBuilder();

            foreach (byte b in hashData)
                hash.Append(b.ToString("X2"));

            return hash.ToString();
        }

        /// <summary>
        /// Creates a HMACSHA512 string
        /// </summary>
        /// <param name="value">Value to be hashed</param>
        /// <param name="key">Key used for the hash</param>
        /// <returns>The HMACSHA512 equivalent of value</returns>
        public static string GetHMACSha512(string value, byte[] key)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            byte[] hashData;
            using (HMACSHA512 sha = new HMACSHA512(key))
            {
                hashData = sha.ComputeHash(data);
            }

            StringBuilder hash = new StringBuilder();

            foreach (byte b in hashData)
                hash.Append(b.ToString("X2"));

            return hash.ToString();
        }

        /// <summary>
        /// Generic extension to compare 2 objects
        /// </summary>
        /// <typeparam name="T">Type to use</typeparam>
        /// <param name="value">Value in</param>
        /// <param name="from">From range</param>
        /// <param name="to">To range</param>
        /// <returns></returns>
        public static bool InRange<T>(this T value, T from, T to) where T : IComparable<T>
        {
            return value.CompareTo(from) >= 1 && value.CompareTo(to) <= -1;
        }

        /// <summary>
        /// Method to compare the 2D distance between two Point structs
        /// </summary>   
        /// <param name="a">The first nullable Point</param>
        /// <param name="b">The second nullable Point</param>
        /// <returns>A double representing the distance between the two points</returns>
        public static double Distance(Point? a, Point? b)
        {
            return Distance((Point)a, (Point)b);
        }

        /// <summary>
        /// Method to compare the 2D distance between two Point structs
        /// </summary>   
        /// <param name="a">The first Point</param>
        /// <param name="b">The second Point</param>
        /// <returns>A double representing the distance between the two points</returns>
        public static double Distance(Point a, Point b)
        {
            int distX = a.X - b.X;
            int distY = a.Y - b.Y;
            return Math.Sqrt((distX * distX) + (distY * distY));
        }

        public static double DistanceTo(this Point a, Point b)
        {
            return Distance(a, b);
        }

        /// <summary>
        /// Creates a string by combining the strings from an array with a separator (default: space) between them
        /// </summary>
        /// <param name="arr">The array to be fused</param>
        /// <param name="startIndex">The index in the array to start at</param>
        /// <returns>A string with all the strings from the startindex appended with a space between them</returns>
        public static string Fuse(this string[] arr, int startIndex = 0, string separator = " ")
        {
            StringBuilder ret = new StringBuilder();
            for (int i = startIndex; i < arr.Length; i++)
            {
                ret.Append(arr[i]);
                if (i != arr.Length - 1)
                    ret.Append(separator);
            }
            return ret.ToString();
        }
    }
}
