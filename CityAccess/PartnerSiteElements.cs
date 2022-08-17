using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class PartnerSiteElements
    {
        public List<CityAccess.Link> Links { get; set; }

        public byte[] Logo { get; set; }

        public byte[] BackgroundImage { get; set; }

        public int PartnerID { get; set; }

    }
}