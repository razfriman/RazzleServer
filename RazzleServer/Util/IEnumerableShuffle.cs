using System;
using System.Collections.Generic;

namespace RazzleServer.Util
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var r = new Random();
            var len = list.Count;
            for (var i = len - 1; i >= 1; --i)
            {
                int j = r.Next(i);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}