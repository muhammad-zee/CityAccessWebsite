using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CityAccess
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using static WebApplication1.App_Start.CustomValidators;

    public class MultipleEvent
    {
        public int ID { get; set; }
        public int serviceID { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public System.DateTime eventDate { get; set; }



        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Start")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public System.TimeSpan startTime { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("End")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public System.TimeSpan endTime { get; set; }


        [DisplayName("Persons")]
        public int maxPersons { get; set; }
        public string stateID { get; set; }

        [DisplayName("Notes")]
        [DataType(DataType.MultilineText)]
        public string notes { get; set; }

        public virtual EventState EventState { get; set; }
        public virtual Service Service { get; set; }


        [DisplayName("Start")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public System.DateTime startDate { get; set; }

        [DisplayName("End")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public System.DateTime endDate { get; set; }



        public string frequency { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public List<CityAccess.Event> Events { get; set; }
    }
}