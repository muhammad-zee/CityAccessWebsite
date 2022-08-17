using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class Agr_Serv_Partn_ServType
    {
        public serviceType ServiceType { get; set; }
        public Service Service { get; set; }
        public Partner Partner { get; set; }
        public Agreement Agreement { get; set; }
        public Partner Partner2 { get; set; }
    }
}