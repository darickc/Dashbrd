using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Dashbrd.Shared.Modules.Weather
{
    public partial class Weather
    {
        private const string ApiBase = "https://api.openweathermap.org/data/2.5/onecall";
        private Timer _timer;

		public Dictionary<string, string> IconTable = new()
        {
            {"01d", "wi-day-sunny"},
            {"02d", "wi-day-cloudy"},
            {"03d", "wi-cloudy"},
            {"04d", "wi-cloudy-windy"},
            {"09d", "wi-showers"},
            {"10d", "wi-rain"},
            {"11d", "wi-thunderstorm"},
            {"13d", "wi-snow"},
            {"50d", "wi-fog"},
            {"01n", "wi-night-clear"},
            {"02n", "wi-night-cloudy"},
            {"03n", "wi-night-cloudy"},
            {"04n", "wi-night-cloudy"},
            {"09n", "wi-night-showers"},
            {"10n", "wi-night-rain"},
            {"11n", "wi-night-thunderstorm"},
            {"13n", "wi-night-snow"},
            {"50n", "wi-night-alt-cloudy-windy"}
        };

        [Inject] private IConfiguration Configuration { get; set; }
		[Inject] public IHttpClientFactory HttpClientFactory { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string ApiId { get; set; }
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
			    var url = $"{ApiBase}?lat={Latitude}&lon={Longitude}&appid={ApiId}&units=imperial&exclude=hourly,minutely";
                try
                {
                    var client = HttpClientFactory.CreateClient();
				    WeatherInfo = await client.GetFromJsonAsync<WeatherInfo>(url);
                }
                catch (Exception e)
                {
                }
            }
        }

        private WeatherInfo GetWeatherInfo()
        {
            var json = GetJson();
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
				PropertyNameCaseInsensitive = true
            };
			var forecast = JsonSerializer.Deserialize<WeatherInfo>(json,options);
            return forecast;
        }

        private string GetJson()
        {
            return @"{
	""lat"": 43.8156,
	""lon"": -111.8524,
	""timezone"": ""America/Boise"",
	""timezone_offset"": -25200,
	""current"": {
				""dt"": 1610750107,
		""sunrise"": 1610722682,
		""sunset"": 1610756128,
		""temp"": 24.55,
		""feels_like"": 16.3,
		""pressure"": 1028,
		""humidity"": 58,
		""dew_point"": 13.28,
		""uvi"": 0.29,
		""clouds"": 1,
		""visibility"": 10000,
		""wind_speed"": 4.61,
		""wind_deg"": 70,
		""weather"": [
			{
					""id"": 800,
				""main"": ""Clear"",
				""description"": ""clear sky"",
				""icon"": ""01d""
			}
		]
	},
	""daily"": [
		{
				""dt"": 1610737200,
			""sunrise"": 1610722682,
			""sunset"": 1610756128,
			""temp"": {
					""day"": 18.79,
				""min"": 7.25,
				""max"": 24.55,
				""night"": 18.03,
				""eve"": 19.31,
				""morn"": 8.17
			},
			""feels_like"": {
					""day"": 12.16,
				""night"": 12.06,
				""eve"": 12.25,
				""morn"": 0.34
			},
			""pressure"": 1035,
			""humidity"": 91,
			""dew_point"": 12.06,
			""wind_speed"": 2.35,
			""wind_deg"": 19,
			""weather"": [
				{
					""id"": 804,
					""main"": ""Clouds"",
					""description"": ""overcast clouds"",
					""icon"": ""04d""
				}
			],
			""clouds"": 100,
			""pop"": 0.02,
			""uvi"": 2.02
		},
		{
				""dt"": 1610823600,
			""sunrise"": 1610809050,
			""sunset"": 1610842602,
			""temp"": {
					""day"": 18.84,
				""min"": 9.09,
				""max"": 21.83,
				""night"": 18.99,
				""eve"": 20.01,
				""morn"": 9.82
			},
			""feels_like"": {
					""day"": 12.92,
				""night"": 12.67,
				""eve"": 13.95,
				""morn"": 3.07
			},
			""pressure"": 1032,
			""humidity"": 95,
			""dew_point"": 15.17,
			""wind_speed"": 1.25,
			""wind_deg"": 331,
			""weather"": [
				{
					""id"": 803,
					""main"": ""Clouds"",
					""description"": ""broken clouds"",
					""icon"": ""04d""
				}
			],
			""clouds"": 68,
			""pop"": 0.03,
			""uvi"": 1.78
		},
		{
				""dt"": 1610910000,
			""sunrise"": 1610895415,
			""sunset"": 1610929077,
			""temp"": {
					""day"": 23.67,
				""min"": 18.12,
				""max"": 24.76,
				""night"": 18.12,
				""eve"": 24.13,
				""morn"": 19.8
			},
			""feels_like"": {
					""day"": 17.49,
				""night"": 11.8,
				""eve"": 18.41,
				""morn"": 14.31
			},
			""pressure"": 1026,
			""humidity"": 97,
			""dew_point"": 21.54,
			""wind_speed"": 2.59,
			""wind_deg"": 288,
			""weather"": [
				{
					""id"": 601,
					""main"": ""Snow"",
					""description"": ""snow"",
					""icon"": ""13d""
				}
			],
			""clouds"": 100,
			""pop"": 0.83,
			""snow"": 2.17,
			""uvi"": 1.71
		},
		{
				""dt"": 1610996400,
			""sunrise"": 1610981778,
			""sunset"": 1611015552,
			""temp"": {
					""day"": 20.07,
				""min"": 12.49,
				""max"": 22.24,
				""night"": 12.69,
				""eve"": 14.79,
				""morn"": 16.57
			},
			""feels_like"": {
					""day"": 14.38,
				""night"": 3.45,
				""eve"": 9.01,
				""morn"": 9.64
			},
			""pressure"": 1030,
			""humidity"": 96,
			""dew_point"": 17.1,
			""wind_speed"": 1.1,
			""wind_deg"": 71,
			""weather"": [
				{
					""id"": 804,
					""main"": ""Clouds"",
					""description"": ""overcast clouds"",
					""icon"": ""04d""
				}
			],
			""clouds"": 99,
			""pop"": 0.13,
			""uvi"": 1.88
		},
		{
				""dt"": 1611082800,
			""sunrise"": 1611068139,
			""sunset"": 1611102029,
			""temp"": {
					""day"": 13.71,
				""min"": 6.19,
				""max"": 17.04,
				""night"": 7.47,
				""eve"": 9.36,
				""morn"": 8.51
			},
			""feels_like"": {
					""day"": 7.05,
				""night"": -0.27,
				""eve"": 2.25,
				""morn"": -0.31
			},
			""pressure"": 1038,
			""humidity"": 91,
			""dew_point"": 7.3,
			""wind_speed"": 1.74,
			""wind_deg"": 32,
			""weather"": [
				{
					""id"": 800,
					""main"": ""Clear"",
					""description"": ""clear sky"",
					""icon"": ""01d""
				}
			],
			""clouds"": 0,
			""pop"": 0,
			""uvi"": 2.03
		},
		{
				""dt"": 1611169200,
			""sunrise"": 1611154497,
			""sunset"": 1611188507,
			""temp"": {
					""day"": 12.63,
				""min"": 4.3,
				""max"": 15.69,
				""night"": 8.42,
				""eve"": 8.01,
				""morn"": 12.27
			},
			""feels_like"": {
					""day"": 6.4,
				""night"": 1.67,
				""eve"": 0.57,
				""morn"": 5.25
			},
			""pressure"": 1028,
			""humidity"": 92,
			""dew_point"": 6.87,
			""wind_speed"": 0.89,
			""wind_deg"": 290,
			""weather"": [
				{
					""id"": 802,
					""main"": ""Clouds"",
					""description"": ""scattered clouds"",
					""icon"": ""03d""
				}
			],
			""clouds"": 28,
			""pop"": 0.02,
			""uvi"": 3
		},
		{
				""dt"": 1611255600,
			""sunrise"": 1611240853,
			""sunset"": 1611274985,
			""temp"": {
					""day"": 17.82,
				""min"": 9.63,
				""max"": 20.91,
				""night"": 14.32,
				""eve"": 17.73,
				""morn"": 10.8
			},
			""feels_like"": {
					""day"": 11.28,
				""night"": 7.95,
				""eve"": 11.88,
				""morn"": 3.7
			},
			""pressure"": 1024,
			""humidity"": 96,
			""dew_point"": 15.06,
			""wind_speed"": 2.24,
			""wind_deg"": 332,
			""weather"": [
				{
					""id"": 600,
					""main"": ""Snow"",
					""description"": ""light snow"",
					""icon"": ""13d""
				}
			],
			""clouds"": 100,
			""pop"": 0.62,
			""snow"": 1.72,
			""uvi"": 3
		},
		{
				""dt"": 1611342000,
			""sunrise"": 1611327208,
			""sunset"": 1611361464,
			""temp"": {
					""day"": 20.5,
				""min"": 12.74,
				""max"": 28.02,
				""night"": 25.9,
				""eve"": 25.88,
				""morn"": 12.92
			},
			""feels_like"": {
					""day"": 13.17,
				""night"": 20.41,
				""eve"": 19.62,
				""morn"": 5.92
			},
			""pressure"": 1019,
			""humidity"": 95,
			""dew_point"": 16.79,
			""wind_speed"": 4,
			""wind_deg"": 35,
			""weather"": [
				{
					""id"": 600,
					""main"": ""Snow"",
					""description"": ""light snow"",
					""icon"": ""13d""
				}
			],
			""clouds"": 100,
			""pop"": 1,
			""snow"": 2.53,
			""uvi"": 3
		}
	]
}";
        }

        
    }
}
