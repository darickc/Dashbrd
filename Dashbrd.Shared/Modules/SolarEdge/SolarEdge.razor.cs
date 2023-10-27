using System;
using System.Collections.Generic;
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
        private const string BaseUrl = "https://monitoringapi.solaredge.com/site/";
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

        private async Task GetPower()
        {
            var start = DateTime.Now.AddHours(-24).ToString("yyyy-M-d HH:mm:ss");
            var end = DateTime.Now.ToString("yyyy-M-d HH:mm:ss");
            var url = $"{BaseUrl}{SiteId}/power?endTime={end}&startTime={start}&api_key={ApiKey}";
            try
            {
                SolarData power;
                if (Test)
                {
                    power = GetData(true).Power;
                }
                else
                {
                    if (MemoryCache.TryGetValue(Power, out SolarData cacheData))
                    {
                        power = cacheData;
                    }
                    else
                    {
                        var client = HttpClientFactory.CreateClient();
                        var data = await client.GetFromJsonAsync<Solar>(url);
                        power = data.Power;

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval - 5));
                        MemoryCache.Set(Power, power, cacheEntryOptions);
                    }
                }

                var total = power.Values.Sum(v => v.Value) / 4 / 1000;
                LineConfig.Options.Title.Text = $"Daily Solar Production ({total: 0.0} kWh)";
                LineConfig.Data.Labels.Clear();
                foreach (var label in power.Values.Select(v => v.DateTime.ToString("HH:mm")))
                {
                    LineConfig.Data.Labels.Add(label);
                }

                var dataset = new LineDataset<float?>(power.Values.Select(v => v?.Value / 1000))
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
            var start = DateTime.Now.AddDays(-6).ToString("yyyy-M-d");
            var end = DateTime.Now.ToString("yyyy-M-d");
            var url = $"{BaseUrl}{SiteId}/energy?timeUnit=DAY&endDate={end}&startDate={start}&api_key={ApiKey}";
            try
            {
                SolarData energy;
                if (Test)
                {
                    energy = GetData(false).Energy;
                }
                else
                {
                    if (MemoryCache.TryGetValue(Energy, out SolarData cacheData))
                    {
                        energy = cacheData;
                    }
                    else
                    {
                        var client = HttpClientFactory.CreateClient();
                        var data = await client.GetFromJsonAsync<Solar>(url);
                        energy = data.Energy;

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval-5));
                        MemoryCache.Set(Energy, energy, cacheEntryOptions);
                    }
                }

                var total = energy.Values.Sum(v => v.Value) / 1000;
                BarConfig.Options.Title.Text = $"Weekly Solar Production ({total: 0.0} kWh)";

                BarConfig.Data.Labels.Clear();
                foreach (var label in energy.Values.Select(v => v.DateTime.ToString("M/d")))
                {
                    BarConfig.Data.Labels.Add(label);
                }

                var dataset = new BarDataset<float?>(energy.Values.Select(v => v?.Value / 1000))
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


        private Solar GetData(bool power)
        {
            var json = power ? PowerJson : EnergyJson;
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true
            };
            var forecast = JsonSerializer.Deserialize<Solar>(json, options);
            return forecast;
        }

        private const string PowerJson = @"{
    ""power"": {
        ""timeUnit"": ""QUARTER_OF_AN_HOUR"",
        ""unit"": ""W"",
        ""measuredBy"": ""INVERTER"",
        ""values"": [
            {
                ""date"": ""2021-01-15 00:00:00"",
                ""value"": null
            },
            {
                ""date"": ""2021-01-15 00:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 00:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 00:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 01:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 01:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 01:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 01:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 02:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 02:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 02:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 02:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 03:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 03:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 03:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 03:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 04:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 04:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 04:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 04:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 05:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 05:15:00"",
                ""value"": 0.0
            },
            {
    ""date"": ""2021-01-15 05:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 05:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 06:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 06:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 06:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 06:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 07:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 07:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 07:30:00"",
                ""value"": 0.0
            },
            {
    ""date"": ""2021-01-15 07:45:00"",
                ""value"": 0.0
            },
            {
    ""date"": ""2021-01-15 08:00:00"",
                ""value"": 61.0
            },
            {
    ""date"": ""2021-01-15 08:15:00"",
                ""value"": 388.0
            },
            {
    ""date"": ""2021-01-15 08:30:00"",
                ""value"": 702.0
            },
            {
    ""date"": ""2021-01-15 08:45:00"",
                ""value"": 1197.0
            },
            {
    ""date"": ""2021-01-15 09:00:00"",
                ""value"": 1413.0
            },
            {
    ""date"": ""2021-01-15 09:15:00"",
                ""value"": 1151.0
            },
            {
    ""date"": ""2021-01-15 09:30:00"",
                ""value"": 1979.0
            },
            {
    ""date"": ""2021-01-15 09:45:00"",
                ""value"": 2187.0
            },
            {
    ""date"": ""2021-01-15 10:00:00"",
                ""value"": 2405.0
            },
            {
    ""date"": ""2021-01-15 10:15:00"",
                ""value"": 2597.0
            },
            {
    ""date"": ""2021-01-15 10:30:00"",
                ""value"": 2880.0
            },
            {
    ""date"": ""2021-01-15 10:45:00"",
                ""value"": 2815.0
            },
            {
    ""date"": ""2021-01-15 11:00:00"",
                ""value"": 2049.0
            },
            {
    ""date"": ""2021-01-15 11:15:00"",
                ""value"": 3413.0
            },
            {
    ""date"": ""2021-01-15 11:30:00"",
                ""value"": 3567.0
            },
            {
    ""date"": ""2021-01-15 11:45:00"",
                ""value"": 2384.0
            },
            {
    ""date"": ""2021-01-15 12:00:00"",
                ""value"": 3412.0
            },
            {
    ""date"": ""2021-01-15 12:15:00"",
                ""value"": 4048.0
            },
            {
    ""date"": ""2021-01-15 12:30:00"",
                ""value"": 2303.0
            },
            {
    ""date"": ""2021-01-15 12:45:00"",
                ""value"": 2052.0
            },
            {
    ""date"": ""2021-01-15 13:00:00"",
                ""value"": 3057.0
            },
            {
    ""date"": ""2021-01-15 13:15:00"",
                ""value"": 1715.0
            },
            {
    ""date"": ""2021-01-15 13:30:00"",
                ""value"": 1913.0
            },
            {
    ""date"": ""2021-01-15 13:45:00"",
                ""value"": 2410.0
            },
            {
    ""date"": ""2021-01-15 14:00:00"",
                ""value"": 2683.0
            },
            {
    ""date"": ""2021-01-15 14:15:00"",
                ""value"": 2413.0
            },
            {
    ""date"": ""2021-01-15 14:30:00"",
                ""value"": 1041.0
            },
            {
    ""date"": ""2021-01-15 14:45:00"",
                ""value"": 1138.0
            },
            {
    ""date"": ""2021-01-15 15:00:00"",
                ""value"": 913.0
            },
            {
    ""date"": ""2021-01-15 15:15:00"",
                ""value"": 703.0
            },
            {
    ""date"": ""2021-01-15 15:30:00"",
                ""value"": 861.0
            },
            {
    ""date"": ""2021-01-15 15:45:00"",
                ""value"": 729.0
            },
            {
    ""date"": ""2021-01-15 16:00:00"",
                ""value"": 253.0
            },
            {
    ""date"": ""2021-01-15 16:15:00"",
                ""value"": 208.0
            },
            {
    ""date"": ""2021-01-15 16:30:00"",
                ""value"": 75.0
            },
            {
    ""date"": ""2021-01-15 16:45:00"",
                ""value"": 53.0
            },
            {
    ""date"": ""2021-01-15 17:00:00"",
                ""value"": 0.0
            },
            {
    ""date"": ""2021-01-15 17:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 17:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 17:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 18:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 18:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 18:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 18:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 19:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 19:15:00"",
                ""value"": 0.0
            },
            {
    ""date"": ""2021-01-15 19:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 19:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 20:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 20:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 20:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 20:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 21:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 21:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 21:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 21:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 22:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 22:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 22:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 22:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 23:00:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 23:15:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 23:30:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-15 23:45:00"",
                ""value"": null
            },
            {
    ""date"": ""2021-01-16 00:00:00"",
                ""value"": null
            }
        ]
    }
}";

        private const string EnergyJson = @"{
    ""energy"": {
        ""timeUnit"": ""DAY"",
        ""unit"": ""Wh"",
        ""measuredBy"": ""INVERTER"",
        ""values"": [
        {
            ""date"": ""2021-01-10 00:00:00"",
            ""value"": 6350.0
        },
        {
            ""date"": ""2021-01-11 00:00:00"",
            ""value"": 17544.0
        },
        {
            ""date"": ""2021-01-12 00:00:00"",
            ""value"": 11039.0
        },
        {
            ""date"": ""2021-01-13 00:00:00"",
            ""value"": 7430.0
        },
        {
            ""date"": ""2021-01-14 00:00:00"",
            ""value"": 22054.0
        },
        {
            ""date"": ""2021-01-15 00:00:00"",
            ""value"": 16281.0
        },
        {
            ""date"": ""2021-01-16 00:00:00"",
            ""value"": null
        }
        ]
    }
}";
    }
}
