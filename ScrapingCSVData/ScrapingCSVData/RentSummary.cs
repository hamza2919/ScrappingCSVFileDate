using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingCSVData
{ 
    public class RentSummary
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Bedrooms { get; set; }
        public string Baths { get; set; }
        public string BuildingType { get; set; }
        public int LookBackDays { get; set; }
        public decimal Mean { get; set; }
        public decimal Median { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Percentile25 { get; set; }
        public decimal Percentile75 { get; set; }
        public decimal StdDev { get; set; }
        public int Samples { get; set; }
        public double RadiusMiles { get; set; }
        public string QuickviewUrl { get; set; }
        public int CreditsRemaining { get; set; }
        public string Token { get; set; }
        public List<Link> Links { get; set; }
    }

    public class Link
    {
        public string Rel { get; set; }
        public string Href { get; set; }
    }

}
