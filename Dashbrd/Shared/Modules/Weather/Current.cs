using System;

namespace Dashbrd.Shared.Modules.Weather
{
    public class Current
    {
        public int Dt { get; set; }
        public int Sunrise { get; set; }
        public int Sunset { get; set; }
        public float Temp { get; set; }
        public float Feels_Like { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public float Dew_Point { get; set; }
        public float Uvi { get; set; }
        public int Clouds { get; set; }
        public int Visibility { get; set; }
        public float Wind_speed { get; set; }
        public int Wind_deg { get; set; }
        public WeatherData[] Weather { get; set; }

        public string NextSunAction()
        {
            var now = DateTime.Now;
            return now >= Sunrise.UnixTimeStampToDateTime() && now < Sunset.UnixTimeStampToDateTime() ? "sunset" : "sunrise";
        }

        public string GetSunriseSunset()
        {
            if (NextSunAction() == "sunset")
            {
                return Sunset.UnixTimeStampToDateTime().ToString("h:mm tt");
            }

            return Sunrise.UnixTimeStampToDateTime().ToString("h:mm tt");
        }
        
        public string CardinalWindDirection()
        {
            if (Wind_deg > 11.25 && Wind_deg <= 33.75)
            {
                return "NNE";
            }
            else if (Wind_deg > 33.75 && Wind_deg <= 56.25)
            {
                return "NE";
            }
            else if (Wind_deg > 56.25 && Wind_deg <= 78.75)
            {
                return "ENE";
            }
            else if (Wind_deg > 78.75 && Wind_deg <= 101.25)
            {
                return "E";
            }
            else if (Wind_deg > 101.25 && Wind_deg <= 123.75)
            {
                return "ESE";
            }
            else if (Wind_deg > 123.75 && Wind_deg <= 146.25)
            {
                return "SE";
            }
            else if (Wind_deg > 146.25 && Wind_deg <= 168.75)
            {
                return "SSE";
            }
            else if (Wind_deg > 168.75 && Wind_deg <= 191.25)
            {
                return "S";
            }
            else if (Wind_deg > 191.25 && Wind_deg <= 213.75)
            {
                return "SSW";
            }
            else if (Wind_deg > 213.75 && Wind_deg <= 236.25)
            {
                return "SW";
            }
            else if (Wind_deg > 236.25 && Wind_deg <= 258.75)
            {
                return "WSW";
            }
            else if (Wind_deg > 258.75 && Wind_deg <= 281.25)
            {
                return "W";
            }
            else if (Wind_deg > 281.25 && Wind_deg <= 303.75)
            {
                return "WNW";
            }
            else if (Wind_deg > 303.75 && Wind_deg <= 326.25)
            {
                return "NW";
            }
            else if (Wind_deg > 326.25 && Wind_deg <= 348.75)
            {
                return "NNW";
            }
            else
            {
                return "N";
            }
        }
    }
}