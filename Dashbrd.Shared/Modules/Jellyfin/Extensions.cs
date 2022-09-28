using System;

namespace Dashbrd.Shared.Modules.Jellyfin;

public static class Extensions
{
    public static string ToText(this TimeSpan span)
    {
        string text = string.Empty;
        var hours = (int)span.TotalHours;
        if (hours > 0)
        {
            text = $"{hours}:";
        }

        text += span.ToString("mm\\:ss");
        return text;
    }
}