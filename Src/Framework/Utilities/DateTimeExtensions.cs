using System;
using System.Runtime.InteropServices;

namespace TrxEE.Utilities
{

    public static class DateTimeExtensions
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
        }
    }
}
