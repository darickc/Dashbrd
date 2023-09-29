using System;

namespace Dashbrd.Shared.Modules.Jellyfin;

public class JellyfinNotificationData
{
    public string DeviceId { get; set; }
    public string DeviceName { get; set; }
    public string ClientName { get; set; }
    public string ItemId { get; set; }
    public string SeriesId { get; set; }
    public double RunTime { get; set; }
    public double PlaybackPosition { get; set; }
    public string Server { get; set; }
    public string Name { get; set; }
    public int Year { get; set; }
    public bool IsPaused { get; set; }
    public string ItemType { get; set; }
    public string SeriesName { get; set; }
    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public TimeSpan RunTimeSpan => TimeSpan.FromTicks((long)RunTime);
    public TimeSpan Position => TimeSpan.FromTicks((long)PlaybackPosition);
    public string LogoData { get; set; }

    public string Thumbnail => $"{Server}/Items/{ItemId}/Images/Primary";
    public string Backdrop => $"{Server}/Items/{SeriesId ?? ItemId}/Images/Backdrop";
    public string Logo => $"{Server}/Items/{SeriesId ?? ItemId}/Images/Logo";
}

