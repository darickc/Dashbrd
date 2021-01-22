using System;
using System.Collections.Generic;
using Dashbrd.Shared.Modules.Weather;

namespace Dashbrd.Shared.Modules.Calendar
{
    public static class CalendarExtensions
    {
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