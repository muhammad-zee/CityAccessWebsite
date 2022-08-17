using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CityAccess
{
    public class ReqLog
    {
        public Agreement Agreement { get; set; }
        public Request Request { get; set; }
        public IEnumerable<CityAccess.RequestLog> ReqLog_List { get; set; }
        public String stateID { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("Operator notes")]
        public String operatorNotes { get; set; }

        public String ClientMailSubject { get; set; }
        public String ClientMailContent { get; set; }

        public String ResponsibleID { get; set; }

        public int Vouch { get; set; }
    }
}