using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfareND
{
    public class Info
    {
        public string title { get; set; }
        public List<object> messages { get; set; }
        public object redirectUrl { get; set; }
        public object callToActionText { get; set; }
        public bool isCampaignCodeInvalid { get; set; }
    }
}
