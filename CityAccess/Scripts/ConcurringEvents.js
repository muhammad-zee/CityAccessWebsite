//Temporary solution for events at the same time in full calendar, keep checking fullcalendar docs

// the way this function is built accounts for object position defined in calendar.cshtml as well as query order in servicesbooked and requested
function cleanArray(requests) {

    var reqLength = rquests.length;
    var event;
    var d1;
    var d2;

    for (var i = 0; i < reqLength; i++)
    {

        d1 = new Date(requests[i][Object.keys(requests[i])[3]]); //position 3 is enddate of event
        d2 = new Date(requests[i + 1][Object.keys(requests[i + 1])[2]]); //position 2 is startdate of event

        if (d2 < d1 == true) {
            //it means they are overlaped 
            //remove second event from object and add an identifier that tells the number of concurring events
            //decrease i so that we can check until no more events are overlaped
        }
    }

}