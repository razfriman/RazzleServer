using System;
using System.Collections.Generic;
using System.Text;

namespace RazzleServer.Common.Util
{
    public static class Extensions
    {
        /// <summary>
        /// Global random against time-based seed mistakes
        /// </summary>
        private static readonly Random Rand = new Random();

        /// <summary>
        /// Generic extension to compare 2 objects
        /// </summary>
        /// <typeparam name="T">Type to use</typeparam>
        /// <param name="inputValue">Value in</param>
        /// <param name="from">From range</param>
        /// <param name="to">To range</param>
        /// <returns></returns>
        public static bool InRange<T>(this T inputValue, T from, T to) where T : IComparable<T> =>
            inputValue.CompareTo(from) >= 1 && inputValue.CompareTo(to) <= -1;

        /// <summary>
        /// Creates a string by combining the strings from an array with a separator (default: space) between them
        /// </summary>
        /// <param name="arr">The array to be fused</param>
        /// <param name="startIndex">The index in the array to start at</param>
        /// <param name="length"></param>
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
                var j = Rand.Next(i);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static string
            ByteArrayToString(this Memory<byte> bArray, bool endingSpace = true, bool noSpace = false) =>
            ByteArrayToString(bArray.ToArray(), endingSpace, noSpace);

        public static string ByteArrayToString(this Span<byte> bArray, bool endingSpace = true, bool noSpace = false) =>
            ByteArrayToString(bArray.ToArray(), endingSpace, noSpace);

        /// <summary>
        /// Converts a byte array to a hexadecimal string
        /// </summary>
        /// <param name="bArray"></param>
        /// <param name="endingSpace"></param>
        /// <param name="noSpace"></param>
        /// <returns></returns>
        public static string ByteArrayToString(this byte[] bArray, bool endingSpace = true, bool noSpace = false)
        {
            //this function is literally the most beautiful thing you've ever seen
            //admit it.
            byte multi = 3;
            if (noSpace)
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
                if (!noSpace)
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
    }
}
