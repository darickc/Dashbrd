using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;


namespace Dashbrd.Shared.Modules.SolarEdge
{
    public partial class SolarEdge
    {
        private const string BaseUrl = "https://monitoringapi.solaredge.com/v2/sites/";
        private const string Power = "SolarEdgePower";
        private const string Energy = "SolarEdgeEnergy";
        private Timer _timer;


        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }

        [Parameter]
        public SolarEdgeType Type { get; set; }

        public string ApiKey { get; set; }
        public string SiteId { get; set; }
        public int UpdateInterval { get; set; } = 30;
        public bool Test { get; set; }

        public BarConfig BarConfig { get; set; } = new()
        {
            Options = new BarOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Weekly Solar Production (kWh)",
                    FontColor = "white"
                },
                Legend = new Legend
                {
                    Display = false
                },
                Scales = new BarScales
                {
                    XAxes = new List<CartesianAxis>()
                    {
                        new BarCategoryAxis
                        {
                            GridLines = new GridLines
                            {
                                Color = ColorUtil.ColorString(255, 255, 255, .25)
                            }
                        }
                    },
                    YAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis()
                        {
                            GridLines = new GridLines()
                            {
                                Color = ColorUtil.ColorString(255, 255, 255, .25)
                            }
                        }
                    }
                }
            }
        };

        public LineConfig LineConfig { get; set; } = new()
        {
            Options = new LineOptions
            {
                Responsive = true,
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Daily Solar Production (kW)",
                    FontColor = "white"
                },
                Legend = new Legend
                {
                    Display = false
                },
                Scales = new Scales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new CategoryAxis()
                        {
                            GridLines = new GridLines
                            {
                                Color = ColorUtil.ColorString(255, 255, 255, .25)
                            }
                        }
                    },
                    YAxes = new List<CartesianAxis>
                    {
                        new LinearCartesianAxis
                        {
                            GridLines = new GridLines()
                            {
                                Color = ColorUtil.ColorString(255, 255, 255, .25)
                            }
                        }
                    }
                }
            }
        };

        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:SolarEdge").Bind(this);
            var span = TimeSpan.FromMinutes(UpdateInterval);
            _timer = new Timer(span.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                await GetSolorData();
                await InvokeAsync(StateHasChanged);
            };
            await GetSolorData();
            _timer.Start();
        }

        private async Task GetSolorData()
        {
            if (Type == SolarEdgeType.Energy)
            {
                await GetEnergy();
            }
            else
            {
                await GetPower();
            }
        }

        private async Task<SolarData> FetchSolarData(string path)
        {
            var client = HttpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
            return await client.GetFromJsonAsync<SolarData>($"{BaseUrl}{SiteId}/{path}");
        }

        private static string FormatDate(DateTimeOffset date) =>
            Uri.EscapeDataString(date.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture));

        private async Task GetPower()
        {
            // HOUR resolution allows at most 24h per request: the current hour plus the previous 23
            var now = DateTimeOffset.Now;
            var start = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Offset).AddHours(-23);
            var path = $"power?from={FormatDate(start)}&to={FormatDate(now)}&resolution=HOUR&unit=KW";
            try
            {
                SolarData power;
                if (Test)
                {
                    power = GetData(true);
                }
                else
                {
                    if (MemoryCache.TryGetValue(Power, out SolarData cacheData))
                    {
                        power = cacheData;
                    }
                    else
                    {
                        power = await FetchSolarData(path);

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval - 5));
                        MemoryCache.Set(Power, power, cacheEntryOptions);
                    }
                }

                // Hourly average kW summed over the hours = kWh
                var total = power.Values.Sum(v => v.Value);
                LineConfig.Options.Title.Text = $"Daily Solar Production ({total: 0.0} kWh)";
                LineConfig.Data.Labels.Clear();
                foreach (var label in power.Values.Select(v => v.Timestamp.ToString("HH:mm")))
                {
                    LineConfig.Data.Labels.Add(label);
                }

                var dataset = new LineDataset<float?>(power.Values.Select(v => v?.Value))
                {
                    BackgroundColor = ColorUtil.ColorString(255, 255, 255, .5),
                    PointStyle = new IndexableOption<PointStyle>(PointStyle.Line)
                };
                LineConfig.Data.Datasets.Clear();
                LineConfig.Data.Datasets.Add(dataset);
            }
            catch
            {
            }
        }

        private async Task GetEnergy()
        {
            var start = new DateTimeOffset(DateTime.Today.AddDays(-6));
            var end = DateTimeOffset.Now;
            var path = $"energy?from={FormatDate(start)}&to={FormatDate(end)}&resolution=DAY&unit=KWH";
            try
            {
                SolarData energy;
                if (Test)
                {
                    energy = GetData(false);
                }
                else
                {
                    if (MemoryCache.TryGetValue(Energy, out SolarData cacheData))
                    {
                        energy = cacheData;
                    }
                    else
                    {
                        energy = await FetchSolarData(path);

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval-5));
                        MemoryCache.Set(Energy, energy, cacheEntryOptions);
                    }
                }

                var total = energy.Values.Sum(v => v.Value);
                BarConfig.Options.Title.Text = $"Weekly Solar Production ({total: 0.0} kWh)";

                BarConfig.Data.Labels.Clear();
                foreach (var label in energy.Values.Select(v => v.Timestamp.ToString("M/d")))
                {
                    BarConfig.Data.Labels.Add(label);
                }

                var dataset = new BarDataset<float?>(energy.Values.Select(v => v?.Value))
                {
                    BackgroundColor = ColorUtil.ColorString(255, 255, 255, .5),
                };
                BarConfig.Data.Datasets.Clear();
                BarConfig.Data.Datasets.Add(dataset);
            }
            catch
            {
            }
        }


        private SolarData GetData(bool power)
        {
            var json = power ? PowerJson : EnergyJson;
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<SolarData>(json, options);
        }

        private const string PowerJson = @"{
    ""period"": {
        ""from"": ""2021-01-15T00:00:00-07:00"",
        ""to"": ""2021-01-15T23:59:59-07:00""
    },
    ""unit"": ""KW"",
    ""resolution"": ""HOUR"",
    ""values"": [
        { ""timestamp"": ""2021-01-15T00:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T01:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T02:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T03:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T04:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T05:00:00-07:00"", ""value"": 0.0 },
        { ""timestamp"": ""2021-01-15T06:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T07:00:00-07:00"", ""value"": 0.0 },
        { ""timestamp"": ""2021-01-15T08:00:00-07:00"", ""value"": 0.587 },
        { ""timestamp"": ""2021-01-15T09:00:00-07:00"", ""value"": 1.683 },
        { ""timestamp"": ""2021-01-15T10:00:00-07:00"", ""value"": 2.674 },
        { ""timestamp"": ""2021-01-15T11:00:00-07:00"", ""value"": 2.853 },
        { ""timestamp"": ""2021-01-15T12:00:00-07:00"", ""value"": 2.954 },
        { ""timestamp"": ""2021-01-15T13:00:00-07:00"", ""value"": 2.274 },
        { ""timestamp"": ""2021-01-15T14:00:00-07:00"", ""value"": 1.819 },
        { ""timestamp"": ""2021-01-15T15:00:00-07:00"", ""value"": 0.802 },
        { ""timestamp"": ""2021-01-15T16:00:00-07:00"", ""value"": 0.147 },
        { ""timestamp"": ""2021-01-15T17:00:00-07:00"", ""value"": 0.0 },
        { ""timestamp"": ""2021-01-15T18:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T19:00:00-07:00"", ""value"": 0.0 },
        { ""timestamp"": ""2021-01-15T20:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T21:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T22:00:00-07:00"", ""value"": null },
        { ""timestamp"": ""2021-01-15T23:00:00-07:00"", ""value"": null }
    ]
}";

        private const string EnergyJson = @"{
    ""period"": {
        ""from"": ""2021-01-10T00:00:00-07:00"",
        ""to"": ""2021-01-16T23:59:59-07:00""
    },
    ""unit"": ""KWH"",
    ""resolution"": ""DAY"",
    ""values"": [
        { ""timestamp"": ""2021-01-10T00:00:00-07:00"", ""value"": 6.35 },
        { ""timestamp"": ""2021-01-11T00:00:00-07:00"", ""value"": 17.544 },
        { ""timestamp"": ""2021-01-12T00:00:00-07:00"", ""value"": 11.039 },
        { ""timestamp"": ""2021-01-13T00:00:00-07:00"", ""value"": 7.43 },
        { ""timestamp"": ""2021-01-14T00:00:00-07:00"", ""value"": 22.054 },
        { ""timestamp"": ""2021-01-15T00:00:00-07:00"", ""value"": 16.281 },
        { ""timestamp"": ""2021-01-16T00:00:00-07:00"", ""value"": null }
    ]
}";
    }
}
