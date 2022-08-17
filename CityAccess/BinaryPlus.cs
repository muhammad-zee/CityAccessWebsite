using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class BinaryPlus : Binary
    {
        public bool LogoPos { get; set; }

        public bool ImgWidth { get; set; }

        public HttpPostedFileBase file { get; set; }
    }
}