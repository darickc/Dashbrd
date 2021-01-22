namespace Dashbrd.Shared.Modules.SolarEdge
{
    public class SolarData
    {
        public string TimeUnit { get; set; }
        public string Unit { get; set; }
        public string MeasuredBy { get; set; }
        public SolarValue[] Values { get; set; }
    }
}