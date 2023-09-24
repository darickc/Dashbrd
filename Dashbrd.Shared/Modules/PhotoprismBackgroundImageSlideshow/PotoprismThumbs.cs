using System.Text.Json.Serialization;

namespace Dashbrd.Shared.Modules.PhotoprismBackgroundImageSlideshow;

public class PotoprismThumbs
{
    [JsonPropertyName("fit_720")]
    public PhotoprismThumb Fit720 { get; set; }

    [JsonPropertyName("fit_1280")]
    public PhotoprismThumb Fit1280 { get; set; }

    [JsonPropertyName("fit_1920")]
    public PhotoprismThumb Fit1920 { get; set; }
}