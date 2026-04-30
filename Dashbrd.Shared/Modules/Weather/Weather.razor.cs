using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Dashbrd.Shared.Modules.Weather
{
    public partial class Weather
    {
        private const string ApiBase = "https://api.open-meteo.com/v1/forecast";
        private const string WeatherCache = "WeatherCache";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private Timer _timer;

        public Dictionary<string, string> IconTable = new()
        {
            { "0d", "wi-day-sunny" },     { "0n", "wi-night-clear" },
            { "1d", "wi-day-sunny" },     { "1n", "wi-night-clear" },
            { "2d", "wi-day-cloudy" },    { "2n", "wi-night-cloudy" },
            { "3d", "wi-cloudy" },        { "3n", "wi-night-cloudy" },
            { "45d", "wi-fog" },          { "45n", "wi-night-fog" },
            { "48d", "wi-fog" },          { "48n", "wi-night-fog" },
            { "51d", "wi-showers" },      { "51n", "wi-night-showers" },
            { "53d", "wi-showers" },      { "53n", "wi-night-showers" },
            { "55d", "wi-showers" },      { "55n", "wi-night-showers" },
            { "56d", "wi-rain-mix" },     { "56n", "wi-night-rain-mix" },
            { "57d", "wi-rain-mix" },     { "57n", "wi-night-rain-mix" },
            { "61d", "wi-rain" },         { "61n", "wi-night-rain" },
            { "63d", "wi-rain" },         { "63n", "wi-night-rain" },
            { "65d", "wi-rain" },         { "65n", "wi-night-rain" },
            { "66d", "wi-rain-mix" },     { "66n", "wi-night-rain-mix" },
            { "67d", "wi-rain-mix" },     { "67n", "wi-night-rain-mix" },
            { "71d", "wi-snow" },         { "71n", "wi-night-snow" },
            { "73d", "wi-snow" },         { "73n", "wi-night-snow" },
            { "75d", "wi-snow" },         { "75n", "wi-night-snow" },
            { "77d", "wi-snow" },         { "77n", "wi-night-snow" },
            { "80d", "wi-showers" },      { "80n", "wi-night-showers" },
            { "81d", "wi-showers" },      { "81n", "wi-night-showers" },
            { "82d", "wi-showers" },      { "82n", "wi-night-showers" },
            { "85d", "wi-snow" },         { "85n", "wi-night-snow" },
            { "86d", "wi-snow" },         { "86n", "wi-night-snow" },
            { "95d", "wi-thunderstorm" }, { "95n", "wi-night-thunderstorm" },
            { "96d", "wi-thunderstorm" }, { "96n", "wi-night-thunderstorm" },
            { "99d", "wi-thunderstorm" }, { "99n", "wi-night-thunderstorm" },
        };

        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int UpdateInterval { get; set; } = 15;
        public bool Test { get; set; }

        public WeatherInfo WeatherInfo { get; set; }


        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:Weather").Bind(this);
            var span = TimeSpan.FromMinutes(UpdateInterval);
            _timer = new Timer(span.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                await UpdateWeather();
                await InvokeAsync(StateHasChanged);
            };
            await UpdateWeather();
            _timer.Start();
        }

        private async Task UpdateWeather()
        {
            if (Test)
            {
                WeatherInfo = GetWeatherInfo();
            }
            else
            {
                if (MemoryCache.TryGetValue(WeatherCache, out WeatherInfo weatherInfo))
                {
                    WeatherInfo = weatherInfo;
                }
                else
                {
                    var url = $"{ApiBase}?latitude={Latitude}&longitude={Longitude}" +
                              "&current=temperature_2m,apparent_temperature,relative_humidity_2m,is_day,wind_speed_10m,wind_direction_10m,weather_code" +
                              "&daily=temperature_2m_max,temperature_2m_min,sunrise,sunset,weather_code" +
                              "&temperature_unit=fahrenheit&wind_speed_unit=mph&timezone=auto&forecast_days=8";
                    try
                    {
                        var client = HttpClientFactory.CreateClient();
                        var response = await client.GetFromJsonAsync<OpenMeteoResponse>(url);
                        WeatherInfo = ToWeatherInfo(response);
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval - 5));
                        MemoryCache.Set(WeatherCache, WeatherInfo, cacheEntryOptions);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static WeatherInfo GetWeatherInfo()
        {
            var response = JsonSerializer.Deserialize<OpenMeteoResponse>(GetJson(), JsonOptions);
            return ToWeatherInfo(response);
        }

        private static WeatherInfo ToWeatherInfo(OpenMeteoResponse r)
        {
            var daily = new Daily[r.Daily.Time.Length];
            for (var i = 0; i < daily.Length; i++)
            {
                daily[i] = new Daily
                {
                    Dt = ToUnix(r.Daily.Time[i]),
                    Sunrise = ToUnix(r.Daily.Sunrise[i]),
                    Sunset = ToUnix(r.Daily.Sunset[i]),
                    Temp = new Temp
                    {
                        Max = r.Daily.Temperature_2m_Max[i],
                        Min = r.Daily.Temperature_2m_Min[i],
                    },
                    Weather = [new WeatherData { Icon = $"{r.Daily.Weather_Code[i]}d" }],
                };
            }

            var dayMarker = r.Current.Is_Day == 1 ? "d" : "n";
            return new WeatherInfo
            {
                Lat = r.Latitude,
                Lon = r.Longitude,
                Timezone = r.Timezone,
                TimezoneOffset = r.Utc_Offset_Seconds,
                Current = new Current
                {
                    Dt = ToUnix(r.Current.Time),
                    Sunrise = daily[0].Sunrise,
                    Sunset = daily[0].Sunset,
                    Temp = r.Current.Temperature_2m,
                    Feels_Like = r.Current.Apparent_Temperature,
                    Humidity = r.Current.Relative_Humidity_2m,
                    Wind_speed = r.Current.Wind_Speed_10m,
                    Wind_deg = r.Current.Wind_Direction_10m,
                    Weather = [new WeatherData { Icon = $"{r.Current.Weather_Code}{dayMarker}" }],
                },
                Daily = daily,
            };
        }

        // Open-Meteo returns local wall-clock time without offset when timezone=auto.
        // Treat as Local so the existing UnixTimeStampToDateTime() display matches the API value.
        private static int ToUnix(string isoLocal)
        {
            var dt = DateTime.SpecifyKind(DateTime.Parse(isoLocal), DateTimeKind.Local);
            return (int)((DateTimeOffset)dt).ToUnixTimeSeconds();
        }

        private static string GetJson()
        {
            return @"{
    ""latitude"": 43.8156,
    ""longitude"": -111.8524,
    ""timezone"": ""America/Boise"",
    ""utc_offset_seconds"": -21600,
    ""current"": {
        ""time"": ""2026-04-29T12:00"",
        ""temperature_2m"": 65.4,
        ""apparent_temperature"": 62.1,
        ""relative_humidity_2m"": 35,
        ""is_day"": 1,
        ""wind_speed_10m"": 8.5,
        ""wind_direction_10m"": 270,
        ""weather_code"": 1
    },
    ""daily"": {
        ""time"": [""2026-04-29"", ""2026-04-30"", ""2026-05-01"", ""2026-05-02"", ""2026-05-03"", ""2026-05-04"", ""2026-05-05"", ""2026-05-06""],
        ""temperature_2m_max"": [70.5, 72.1, 65.8, 60.2, 55.4, 58.9, 64.3, 68.7],
        ""temperature_2m_min"": [40.2, 42.5, 38.1, 35.6, 32.4, 33.8, 36.5, 41.2],
        ""sunrise"": [""2026-04-29T06:42"", ""2026-04-30T06:41"", ""2026-05-01T06:40"", ""2026-05-02T06:38"", ""2026-05-03T06:37"", ""2026-05-04T06:36"", ""2026-05-05T06:35"", ""2026-05-06T06:34""],
        ""sunset"": [""2026-04-29T20:35"", ""2026-04-30T20:36"", ""2026-05-01T20:37"", ""2026-05-02T20:38"", ""2026-05-03T20:39"", ""2026-05-04T20:40"", ""2026-05-05T20:41"", ""2026-05-06T20:42""],
        ""weather_code"": [1, 2, 3, 61, 71, 80, 95, 0]
    }
}";
        }


    }
}
