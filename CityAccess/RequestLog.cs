//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CityAccess
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class RequestLog
    {
        public int ID { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:dd/MM/yyyy}")]
        public System.DateTime Date { get; set; }
        public string Time { get; set; }
        public int requestID { get; set; }
        public Nullable<int> userID { get; set; }

        [DataType(DataType.MultilineText)]
        public string notes { get; set; }

        public virtual Request Request { get; set; }
        public virtual User User { get; set; }
    }
}
