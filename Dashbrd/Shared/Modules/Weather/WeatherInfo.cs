namespace Dashbrd.Shared.Modules.Weather
{
    public class WeatherInfo
    {
        public float Lat { get; set; }
        public float Lon { get; set; }
        public string Timezone { get; set; }
        public int TimezoneOffset { get; set; }
        public Current Current { get; set; }
        public Daily[] Daily { get; set; }
    }
}