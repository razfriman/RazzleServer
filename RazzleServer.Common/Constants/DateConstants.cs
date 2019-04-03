using System;

namespace RazzleServer.Common.Constants
{
    public static class DateConstants
    {
        public static readonly DateTime Permanent = new DateTime(2079, 1, 1, 12, 0, 0);
        
        public static DateTime PermanentBanDate => DateTime.Now.AddYears(2);
    }
}
