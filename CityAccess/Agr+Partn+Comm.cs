using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class Agr_Partn_Comm
    {
        //public commissionType Commission { get; set; }
        public Partner Partner { get; set; }
        public Agreement Agreement { get; set; }
        public String BaseService { get; set; }
        public ServiceImage serviceImage { get; set;}
        public PartnerLogo PartnerLogo { get; set; } 
        public String Label { get; set; }
    }
}