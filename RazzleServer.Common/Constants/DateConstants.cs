using System;

namespace RazzleServer.Common.Constants
{
    public static class DateConstants
    {
        public static readonly DateTime Permanent = new DateTime(2079, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static DateTime PermanentBanDate => DateTime.UtcNow.AddYears(2);

        public static readonly DateTime MapleDateOffset = new DateTime(1970, 1, 1, 11, 0, 0, DateTimeKind.Utc);
    }
}
