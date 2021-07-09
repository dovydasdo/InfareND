using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfareND
{
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
}
