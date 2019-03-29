using System;

namespace RazzleServer.Common.Util
{
    public static class MapleFormatHelper
    {
        /// <summary>
        /// Converts ticks from a DateTime object into Windows file time
        /// </summary>
        /// <param name="timeStamp">The UTC timestamp to be converted to Windows file time</param>
        /// <returns></returns>
        public static long GetMapleTimeStamp(long timeStamp) //timestamp in ticks, -1 -2 and -3 are reserved
        {
            switch (timeStamp)
            {
                case -1:
                    return 0x217E646BB058000; //1-1-2079 0:00:00
                case -2:
                    return 0x14F373BFDE04000; //1-1-1900 0:00:00
                case -3:
                    return 0x217E57D909BC000;
                default:
                    return new DateTime(timeStamp).ToFileTimeUtc();
            }
        }

        public static long GetMapleTimeStamp(DateTime date) => date.ToFileTimeUtc();

        public static DateTime GetDateTimeFromMapleTimeStamp(long mapleTimeStamp) =>
            DateTime.FromFileTimeUtc(mapleTimeStamp);

        public static int GetCurrentDate() => int.Parse(DateTime.UtcNow.ToString("yyyyMMddhh"));
    }
}
