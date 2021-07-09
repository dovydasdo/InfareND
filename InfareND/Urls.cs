using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfareND
{
    class Urls
    {
        public static string DatesUrl = "https://www.norwegian.com/api/fare-calendar/calendar?adultCount=1&destinationAirportCode=FCO&originAirportCode=OSL&outboundDate=2021-11-01&tripType=1&currencyCode=USD&languageCode=en-US&pageId=258774&eventType=init";
        public static string FlightUrl = "https://www.norwegian.com/us/ipc/availability/avaday?AdultCount=1&A_City=FCO&D_City=OSL&D_Month=202111&D_Day={DAYNUM}&IncludeTransit=false&TripType=1&CurrencyCode=USD&dFare=66";
    }
}
