using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfareND
{
    class Date
    {
        public string date { get; set; }
        public string displayPrice { get; set; }
        public bool isAgreementPrice { get; set; }
        public float price { get; set; }
        public float standardDisplayPrice { get; set; }
        public float standardPrice { set; get; }
        public float transitCount { set; get; }
    }
}
