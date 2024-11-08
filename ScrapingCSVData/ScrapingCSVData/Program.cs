using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ScrapingCSVData
{



    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string  filePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "LastTimeScrapedData.txt");
        public static DateTime NewLinkDate;
        static async Task Main(string[] args)
        {
            try
            {
                 
               

                    var csvResponse = await GetCSVFILEResponseFromThirdPartyApi();

               

                    if (csvResponse == null)
                {
                    Console.ReadLine();
                    return;
                }

                    List<ScrapedData> listOfScrapedData = await GetListOfScrapedData(csvResponse);


                if (listOfScrapedData == null)
                {

                Console.ReadLine();
                    return;
                }

                    //Output the list of data
                    //foreach (var data in listOfScrapedData)
                    //{
                    //    Console.WriteLine($"Address: {data.AddressStreet}, Lat {data.Latitude} , Lang {data.Longitude} ");

                    //    Console.WriteLine("-------------------------------");
                    //}

                    var rentSummaries = await PassListToRentoMeterApiToGetRentSummary(listOfScrapedData);


                if (rentSummaries == null)
                {
                Console.ReadLine();
                    return;
                }


                    string htmlContent = GenerateHtmlTemplate(rentSummaries);
                    Console.WriteLine(htmlContent);


                if (htmlContent == null) { 
                Console.ReadLine();
                    return; 
                
                }

                bool IsSended = SendHtmlContextToGmail(htmlContent);

                if (IsSended == true)
                    await SaveDataintoLocalFile(NewLinkDate);
                
                    
                  
                    Console.ReadLine(); 
                
            }
            catch (Exception exception)
            {
                Console.WriteLine($"An error occurred: {exception.Message}");
            }
        }

        private static bool SendHtmlContextToGmail(string htmlContent)
        {

            /// implement gamil sending here 
            return true;
        }

        private static async Task<HttpResponseMessage> GetCSVFILEResponseFromThirdPartyApi()
        {
            string pageUrl = "https://nc-actives.netlify.app/";
            var pageResponse = await httpClient.GetAsync(pageUrl);

            if (!pageResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to retrieve page. Status code: {(int)pageResponse.StatusCode}, Reason: {pageResponse.ReasonPhrase}");
                return null;
            }

            var pageContent = await pageResponse.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            // Assuming the CSV link is the first link found on the page
            var csvLinkNode = doc.DocumentNode.SelectSingleNode("//a[contains(@href, '.csv')]");
            if (csvLinkNode == null)
            {
                Console.WriteLine("CSV file link not found on the page.");
                return null;
            }

            string csvUrl = csvLinkNode.GetAttributeValue("href", string.Empty);
            if (string.IsNullOrEmpty(csvUrl))
            {
                Console.WriteLine("Invalid CSV file link.");
                return null;
            }

            // Extract the date from the href attribute using a regular expression
            var dateMatch = Regex.Match(csvUrl, @"\d{4}-\d{2}-\d{2}");
            if (!dateMatch.Success)
            {
                Console.WriteLine("Date not found in the CSV file link.");
                return null;
            }
            DateTime dateNewRecord = Convert.ToDateTime(dateMatch.Value);

            string locallyStoredDate = await GetLocallyStoreDate();

            if (string.IsNullOrEmpty(locallyStoredDate))
            {
                // today - 1 day and add date this is test purpose for firsttime testing
                await SaveDataintoLocalFile(dateNewRecord.AddDays(-1));
                
            }
            else
            if (Convert.ToDateTime(locallyStoredDate) == dateNewRecord)
            {
                Console.WriteLine($" {dateNewRecord.ToShortDateString()} record of this date is already sended");
                return null;
            }


            NewLinkDate = dateNewRecord;




            // Check if the csvUrl is relative
            if (!Uri.IsWellFormedUriString(csvUrl, UriKind.Absolute))
            {
                csvUrl = new Uri(new Uri(pageUrl), csvUrl).ToString();
            }

            // Fetch and parse the CSV file
            var csvResponse = await httpClient.GetAsync(csvUrl);
            if (!csvResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to retrieve CSV file. Status code: {(int)csvResponse.StatusCode}, Reason: {csvResponse.ReasonPhrase}");
                return null;
            }
            return csvResponse;
        }

        private static async Task SaveDataintoLocalFile(DateTime date)
        {
            try
            {
                // Write the text to the file asynchronously using StreamWriter
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    await writer.WriteAsync(date.ToShortDateString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            //try
            //{
            //    // Write the text to the file asynchronously
            //    await File.WriteAllTextAsync(filePath, date.AddDays(-1).ToShortDateString());
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"An error occurred: {ex.Message}");
            //    return;
            //}
        }

        
        private static async Task<string> GetLocallyStoreDate()
        {
             
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File not found.");
                    return null;
                }

                // Read the text from the file asynchronously
                string content = File.ReadAllText(filePath);
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }

        }

        public static async Task<List<ScrapedData>> GetListOfScrapedData(HttpResponseMessage csvResponse)
        {
            var stream = await csvResponse.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var listOfData = new List<ScrapedData>();
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                var scrapedData = csv.GetRecord<ScrapedData>();
                if (scrapedData.PriceToRentalZestimateRatio >= 95)
                    listOfData.Add(scrapedData);
            }
            return listOfData;

        }



        private async static Task<List<RentSummary>> PassListToRentoMeterApiToGetRentSummary(List<ScrapedData> listOfScrapedData)
        {
            string apiKey = "rkUAgETu5xv76SXmJGTuQg";
            List<RentSummary> rentSummaries = new List<RentSummary>();
            foreach (var item in listOfScrapedData.Take(5))
            {
                string address = item.AddressStreet.ToString();
                string bedrooms = item.BedroomCount.ToString();
                string latitude = item.Latitude.ToString();
                string longitude = item.Longitude.ToString();

                string requestUrl = $"https://www.rentometer.com/api/v1/summary?api_key={apiKey}&bedrooms={bedrooms}";

                if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
                {
                    requestUrl += $"&latitude={latitude}&longitude={longitude}";
                }
                else
                {
                    requestUrl += $"&address={Uri.EscapeDataString(address)}";
                }

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);


                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($" Address {address} Lat{latitude} Lang{longitude} Error response: {errorResponse} status code: {(int)response.StatusCode}");
                        continue;
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(responseBody);

                    RentSummary rentSummary = new RentSummary
                    {
                        Address = jsonResponse["address"]?.ToString(),
                        Latitude = jsonResponse["latitude"]?.ToObject<double>() ?? 0,
                        Longitude = jsonResponse["longitude"]?.ToObject<double>() ?? 0,
                        Bedrooms = jsonResponse["bedrooms"]?.ToObject<int>() ?? 0,
                        Baths = jsonResponse["baths"]?.ToString(),
                        BuildingType = jsonResponse["building_type"]?.ToString(),
                        LookBackDays = jsonResponse["look_back_days"]?.ToObject<int>() ?? 0,
                        Mean = jsonResponse["mean"]?.ToObject<decimal>() ?? 0,
                        Median = jsonResponse["median"]?.ToObject<decimal>() ?? 0,
                        Min = jsonResponse["min"]?.ToObject<decimal>() ?? 0,
                        Max = jsonResponse["max"]?.ToObject<decimal>() ?? 0,
                        Percentile25 = jsonResponse["percentile_25"]?.ToObject<decimal>() ?? 0,
                        Percentile75 = jsonResponse["percentile_75"]?.ToObject<decimal>() ?? 0,
                        StdDev = jsonResponse["std_dev"]?.ToObject<decimal>() ?? 0,
                        Samples = jsonResponse["samples"]?.ToObject<int>() ?? 0,
                        RadiusMiles = jsonResponse["radius_miles"]?.ToObject<double>() ?? 0,
                        QuickviewUrl = jsonResponse["quickview_url"]?.ToString(),
                        CreditsRemaining = jsonResponse["credits_remaining"]?.ToObject<int>() ?? 0,
                        Token = jsonResponse["token"]?.ToString(),
                        Links = jsonResponse["links"]?.ToObject<List<Link>>()
                    };

                    rentSummaries.Add(rentSummary);

                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("HTTP Request Failed");
                    Console.WriteLine($"Message :{e.Message} ");
                }
                finally
                {
                    // Optional: Code that will run always
                }
            }

            return rentSummaries;

        }

        static string GenerateHtmlTemplate(List<RentSummary> rentSummaries)
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Rent Summaries</title>
    <style>
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 8px 12px; border: 1px solid #ccc; text-align: left; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h1>Rent Summaries</h1>
    <table>
        <thead>
            <tr>
                <th>Address</th>
                <th>Latitude</th>
                <th>Longitude</th>
                <th>Mean</th>
                <th>Median</th>
                <th>Min</th>
                <th>Max</th>
                <th>25th Percentile</th>
                <th>75th Percentile</th>
            </tr>
        </thead>
        <tbody>";

            foreach (var summary in rentSummaries)
            {
                html += $@"
            <tr>
                <td>{summary.Address}</td>
                <td>{summary.Latitude}</td>
                <td>{summary.Longitude}</td>
                <td>{summary.Mean}</td>
                <td>{summary.Median}</td>
                <td>{summary.Min}</td>
                <td>{summary.Max}</td>
                <td>{summary.Percentile25}</td>
                <td>{summary.Percentile75}</td>
            </tr>";
            }

            html += @"
        </tbody>
    </table>
</body>
</html>";

            return html;
        }
    }



}

 
 

