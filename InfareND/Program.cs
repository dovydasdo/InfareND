using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InfareND
{
    public class Day
    {
        public DateTime date { get; set; }
        public double price { get; set; }
        public double standardPrice { get; set; }
        public string displayPrice { get; set; }
        public string standardDisplayPrice { get; set; }
        public bool isSoldOut { get; set; }
        public int transitCount { get; set; }
        public bool isAgreementPrice { get; set; }
    }

    public class Outbound
    {
        public List<Day> days { get; set; }
    }

    public class Info
    {
        public string title { get; set; }
        public List<object> messages { get; set; }
        public object redirectUrl { get; set; }
        public object callToActionText { get; set; }
        public bool isCampaignCodeInvalid { get; set; }
    }

    public class Root
    {
        public Outbound outbound { get; set; }
        public object inbound { get; set; }
        public Info info { get; set; }
        public string gtmEventAsJson { get; set; }
        public List<object> subsidyDiscounts { get; set; }
        public string currencyCode { get; set; }
        public bool isPremiumAvailable { get; set; }
        public string metaTitle { get; set; }
        public string metaDescription { get; set; }
        public string pageUrlPath { get; set; }
        public string eventType { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            startCrawlerasync();
            Console.ReadLine();
        }

        private static async Task startCrawlerasync()
        {

            string datesUrl = "https://www.norwegian.com/api/fare-calendar/calendar?adultCount=1&destinationAirportCode=FCO&originAirportCode=OSL&outboundDate=2021-11-01&tripType=1&currencyCode=USD&languageCode=en-US&pageId=258774&eventType=init";

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            HttpClient httpClient = new HttpClient(handler);
            HttpResponseMessage resp = httpClient.GetAsync(datesUrl).Result;

            Uri uri = new Uri(datesUrl);

            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();

            string cookieValue = GetCookieString(responseCookies);

            httpClient = SetHeaders(httpClient, cookieValue);

            List<Day> days = null;
            try
            {
                var response = await httpClient.GetStringAsync(datesUrl);
                dynamic obj = JsonConvert.DeserializeObject(response);
                var items = obj.ToObject<Root>();
                days = items.outbound.days;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }

            List<int> validDates = GetValidDays(days);

            string flightUrl = "https://www.norwegian.com/us/ipc/availability/avaday?AdultCount=1&A_City=FCO&D_City=OSL&D_Month=202111&D_Day={DAYNUM}&IncludeTransit=false&TripType=1&CurrencyCode=USD&dFare=66";
            List<Flight> flights = new List<Flight>();

            double taxes = 0.0;

            foreach (int date in validDates)
            {
                try
                {
                    string day = GetDay(date);
                    string url = flightUrl.Replace("{DAYNUM}", day);
                    Thread.Sleep(500);

                    var response = await httpClient.GetStringAsync(url);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);
                    var timeAndPrice = htmlDoc.DocumentNode.Descendants("tr").Where(node => node.GetAttributeValue("class", "").Contains("rowinfo1")).ToList().First();
                    var ariportNames = htmlDoc.DocumentNode.Descendants("tr").Where(node => node.GetAttributeValue("class", "").Contains("rowinfo2")).ToList().First();

                    double lowFarePrice = Convert.ToDouble(timeAndPrice.SelectNodes("//td[contains(@class, 'fareselect standardlowfare')]").FirstOrDefault().FirstChild.FirstChild.InnerText);
                    double lowFarePlusPrice = Convert.ToDouble(timeAndPrice.SelectNodes("//td[contains(@class, 'fareselect standardlowfareplus')]").FirstOrDefault().FirstChild.FirstChild.InnerText);
                    double flexPrice = Convert.ToDouble(timeAndPrice.SelectNodes("//td[contains(@class, 'fareselect standardflex endcell')]").FirstOrDefault().FirstChild.FirstChild.InnerText);

                    double cheapestPrice = GetCheapest(lowFarePrice, lowFarePlusPrice, flexPrice);


                    if (taxes == 0.0)
                    {
                        taxes = Convert.ToDouble(htmlDoc.DocumentNode.SelectNodes("//td[contains(@class, 'rightcell emphasize')]").LastOrDefault().InnerText.Replace("$", ""));
                    }

                    cheapestPrice = cheapestPrice - taxes;

                    string fullDepDate =
                        "2021-11-" +
                        day +
                        " " +
                        timeAndPrice.SelectNodes("//td[contains(@class, 'depdest')]").FirstOrDefault().FirstChild.InnerText
                        ;

                    string fullArrDate =
                        "2021-11-" +
                        day +
                        " " +
                        timeAndPrice.SelectNodes("//td[contains(@class, 'arrdest')]").FirstOrDefault().FirstChild.InnerText
                        ;
                    DateTime depDate = DateTime.ParseExact(fullDepDate, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime arrDate = DateTime.ParseExact(fullArrDate, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    Flight flight = new Flight
                    {
                        DepartureTime = depDate,
                        ArrivalTime = arrDate,
                        Price = cheapestPrice,
                        Taxes = taxes,
                        DepartureAirport = ariportNames.Descendants("td").Where(node => node.GetAttributeValue("class", "").Equals("depdest")).ToList().First().FirstChild.InnerText,
                        ArrivalAirport = ariportNames.Descendants("td").Where(node => node.GetAttributeValue("class", "").Equals("arrdest")).ToList().First().FirstChild.InnerText
                    };

                    flights.Add(flight);

                }
                catch (Exception e)
                {
                    throw;
                }
            }
            using (StreamWriter file = File.CreateText(@"C:\Users\dovyd\source\repos\InfareND\InfareND\flights.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, flights);
            }
        }

        private static string GetCookieString(IEnumerable<Cookie> responseCookies)
        {
            string cookieValue = "";

            foreach (Cookie cookie in responseCookies)
            {
                cookieValue = cookieValue + cookie.Name + "=" + cookie.Value + "; ";
            }

            return cookieValue;
        }

        private static HttpClient SetHeaders(HttpClient httpClient, string cookieValue)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.9,en;q=0.8,ru;q=0.7,pl;q=0.6");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://www.norwegian.com/us/low-fare-calendar/?AdultCount=1&A_City=FCO&D_City=OSL&D_Month=202111&D_Day=01&IncludeTransit=false&TripType=1");
                httpClient.DefaultRequestHeaders.Add("cookie", cookieValue);
            }
            catch (Exception e)
            {
                throw;
            }

            return httpClient;
        }

        private static List<int> GetValidDays(List<Day> days)
        {
            List<int> validDates = new List<int>();

            int index = 0;
            foreach (Day day in days)
            {
                if (day.price != 0)
                {
                    validDates.Add(index + 1);
                }
                index++;
            }

            return validDates;
        }

        private static string GetDay(int date)
        {
            string day = date.ToString();

            if (day.Length == 1)
            {
                day = "0" + day;
            }

            return day;
        }

        private static double GetCheapest(double lowFarePrice, double lowFarePlusPrice, double flexPrice)
        {
            double cheapestPrice = lowFarePrice;

            if (cheapestPrice > lowFarePlusPrice)
            {
                cheapestPrice = lowFarePlusPrice;
            }
            if (cheapestPrice > flexPrice)
            {
                cheapestPrice = flexPrice;
            }

            return cheapestPrice;
        }

    }
}
