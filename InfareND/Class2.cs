using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfareND
{
    class Calendar
    {
        private string currencyCode { get; set; }
        private string eventType { get; set; }
        private string gtmEventAsJson { get; set; }
        private string inbound { get; set; }
        private string info { get; set; }
        private bool isPremiumAvailable { get; set; }
        private string metaDescription { get; set; }
        private string metaTitle { get; set; }
        private Dictionary<string, Date[]> outbound { get; set; }
        private string pageUrlPath { get; set; }
        private string[] subsidyDiscounts { get; set; }

    }
}
