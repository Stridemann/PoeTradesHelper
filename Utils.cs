using System;

namespace PoeTradesHelper
{
    public static class Utils
    {
        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours}h";

            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes}m";

            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes}m";

            return $"{timeSpan.Seconds}sec";
        }
    }
}