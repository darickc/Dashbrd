namespace Dashbrd.Shared.Modules.Weather
{
    public class OpenMeteoResponse
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Timezone { get; set; }
        public int Utc_Offset_Seconds { get; set; }
        public OpenMeteoCurrent Current { get; set; }
        public OpenMeteoDaily Daily { get; set; }
    }

    public class OpenMeteoCurrent
    {
        public string Time { get; set; }
        public float Temperature_2m { get; set; }
        public float Apparent_Temperature { get; set; }
        public int Relative_Humidity_2m { get; set; }
        public int Is_Day { get; set; }
        public float Wind_Speed_10m { get; set; }
        public int Wind_Direction_10m { get; set; }
        public int Weather_Code { get; set; }
    }

    public class OpenMeteoDaily
    {
        public string[] Time { get; set; }
        public float[] Temperature_2m_Max { get; set; }
        public float[] Temperature_2m_Min { get; set; }
        public string[] Sunrise { get; set; }
        public string[] Sunset { get; set; }
        public int[] Weather_Code { get; set; }
    }
}
