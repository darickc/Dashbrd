using System;
using System.Globalization;

namespace Dashbrd.Shared.Modules.SolarEdge
{
    public class SolarValue
    {
        private string _date;

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                DateTime = DateTime.ParseExact(Date, "yyyy-MM-dd HH:mm:ss", new DateTimeFormatInfo());
            }
        }

        public DateTime DateTime { get; private set; }
        public float? Value { get; set; }
    }
}