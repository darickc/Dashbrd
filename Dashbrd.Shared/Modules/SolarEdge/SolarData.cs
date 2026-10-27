namespace Dashbrd.Shared.Modules.SolarEdge
{
    public class SolarData
    {
        public string Unit { get; set; }
        public string Resolution { get; set; }
        public SolarValue[] Values { get; set; }
    }
}
