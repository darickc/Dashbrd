using System;
using System.Collections.Generic;

namespace Dashbrd.Shared.Modules.Weather
{
    public static class WeatherExtensions
    {
        public static string Degree(this string value)
        {
            return $"{value}°";
        }

        public static string Degree(this float value)
        {
            return $"{value: 0.0}°";
        }

        public static DateTime UnixTimeStampToDateTime(this int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string Pluralize(this string text, int count)
        {
            var value = count == 1 ? "" : "s";
            return $"{text}{value}";
        }

        public static string ToText(this TimeSpan span)
        {
            var temp = new List<string>();
            if (span.Hours > 0)
            {
                temp.Add($"{span.Hours} {"hour".Pluralize(span.Hours)}");
            }

            if (span.Minutes > 0)
            {
                temp.Add($"{span.Minutes} {"hour".Pluralize(span.Minutes)}");
            }

            return string.Join(" ", temp);
        }
    }
}