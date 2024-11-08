using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapingCSVData
{
    internal class ScrapedData
    {
        [Name("address_street")]
        public string AddressStreet { get; set; }

        [Name("address_city")]
        public string AddressCity { get; set; }

        [Name("address_county")]
        public string AddressCounty { get; set; }

        [Name("address_state")]
        public string AddressState { get; set; }

        [Name("address_zip")]
        public string AddressZip { get; set; }

        [Name("bathroomCount")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal BathroomCount { get; set; }

        [Name("bedroomCount")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal BedroomCount { get; set; }

        [Name("fullBathroomCount")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal FullBathroomCount { get; set; }

        [Name("halfBathroomCount")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal HalfBathroomCount { get; set; }

        [Name("latitude")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal Latitude { get; set; }

        [Name("longitude")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal Longitude { get; set; }

        [Name("lotSizeSquareFeet")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal LotSizeSquareFeet { get; set; }

        [Name("price")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal Price { get; set; }

        [Name("zestimate")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal Zestimate { get; set; }

        [Name("rentzestimate")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal RentZestimate { get; set; }

        [Name("price_to_rental_zestimate_ratio")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal PriceToRentalZestimateRatio { get; set; }

        [Name("daysOnMarket")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal DaysOnMarket { get; set; }

        [Name("status")]
        public string Status { get; set; }

        [Name("yearBuilt")]
        [TypeConverter(typeof(CustomDecimalConverter))]
        public decimal YearBuilt { get; set; }

        [Name("propertyType")]
        public string PropertyType { get; set; }

        [Name("propertySubtype")]
        public string PropertySubtype { get; set; }

        [Name("zillow_url")]
        public string ZillowUrl { get; set; }

        [Name("parcel_number")]
        public string ParcelNumber { get; set; }

        [Name("scrape_date")]
        public DateTime ScrapeDate { get; set; }
    }
}
