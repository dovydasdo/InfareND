using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
