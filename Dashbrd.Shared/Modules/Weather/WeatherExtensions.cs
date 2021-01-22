using System;

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
    }
}