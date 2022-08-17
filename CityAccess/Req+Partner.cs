using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class Req_Partner
    {
        //dividido provavelmente para diferenciar requests para transfers de outros requests
        public Req_forTransfer req_ForTransfer { get; set; }
        public Request Request { get; set; }
        public Partner Partner { get; set; }
        public Agreement Agreement { get; set; }
        public serviceType ServiceType { get; set; }
        public IEnumerable<CityAccess.State> States { get; set; }
    }
}