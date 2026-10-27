using System;

namespace Dashbrd.Shared.Modules.SolarEdge
{
    public class SolarValue
    {
        public DateTimeOffset Timestamp { get; set; }
        public float? Value { get; set; }
    }
}
