using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class UsLog
    {
        public IEnumerable<CityAccess.UserLog> UserLog_List { get; set; }
        public User user { get; set; }
    }
}