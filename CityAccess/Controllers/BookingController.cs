using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CityAccess.Controllers
{
    public class BookingController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Booking
        /// <summary>
        /// Sets up the booking screen in order to book a request
        /// </summary>
        /// <param name="agID">If the booking comes from partner site this argument is active and jas value</param>
        /// <returns>the view with the booking form to fill</returns>
        public ActionResult Index(int? agID)
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null && agID == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var origin = false;

            if (agID == null)
            {
                agID = (int)Session["AgreementID"];
            }
            else
            {
                Session["AgreementID"] = agID;
                origin = true;
            }

            Agreement ag = db.Agreements.Where(z => z.ID == agID).FirstOrDefault();
            var serv = db.Services.Where(x => x.ID == ag.serviceID).FirstOrDefault();
            var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
            var servImg = db.ServiceImages.Where(j => j.serviceID == serv.ID && j.sequenceNR == 1).FirstOrDefault();

            if (ag.Override1 != null)
            {
                if (ag.Override1 == true)
                {
                    ViewBag.Override = true;
                }
            }
            else
            {
                if (serv.Override1 == true)
                {
                    ViewBag.Override = true;
                }
            }
            if (ag.priceType != null)
            {
                if (ag.priceType == 8)
                {
                    ViewBag.pType = "fixed";
                }
                if (ag.priceType == 9)
                {
                    ViewBag.pType = "open";
                }
                if (ag.priceType == 10)
                {
                    ViewBag.pType = "person";
                }
                if (ag.priceType == 11)
                {
                    ViewBag.pType = "formula";
                }
            }
            else
            {
                if (serv.priceType == 8)
                {
                    ViewBag.pType = "fixed";
                }
                if (serv.priceType == 9)
                {
                    ViewBag.pType = "open";
                }
                if (serv.priceType == 10)
                {
                    ViewBag.pType = "person";
                }
                if (serv.priceType == 11)
                {
                    ViewBag.pType = "formula";
                }
            }



            if (servType.hasReturn == true)
            {
                ViewBag.Control = "return";
            }

            if(servType.isTransfer != true)
            {
                ViewBag.NotTransfer = "notTransfer";
            }




            if(serv.field1IsActive == true && serv.field1IsMandatory == true)
            {
                ViewBag.extra1 = "mandatory1";
            }

            if (serv.field2IsActive == true && serv.field2IsMandatory == true)
            {
                ViewBag.extra2 = "mandatory2";
            }

            if (serv.field3IsActive == true && serv.field3IsMandatory == true)
            {
                ViewBag.extra3 = "mandatory3";
            }

            if (serv.field4IsActive == true && serv.field4IsMandatory == true)
            {
                ViewBag.extra4 = "mandatory4";
            }

            if (serv.field5IsActive == true && serv.field5IsMandatory == true)
            {
                ViewBag.extra5 = "mandatory5";
            }
            if (serv.field6IsActive == true && serv.field6IsMandatory == true)
            {
                ViewBag.extra6 = "mandatory6";
            }

            if (serv.field7IsActive == true && serv.field7IsMandatory == true)
            {
                ViewBag.extra7 = "mandatory7";
            }
            if (serv.field8IsActive == true && serv.field8IsMandatory == true)
            {
                ViewBag.extra8 = "mandatory8";
            }
            if (serv.field9IsActive == true && serv.field9IsMandatory == true)
            {
                ViewBag.extra9 = "mandatory9";
            }
            if (serv.field10IsActive == true && serv.field10IsMandatory == true)
            {
                ViewBag.extra10 = "mandatory10";
            }
            if (serv.field11IsActive == true && serv.field11IsMandatory == true)
            {
                ViewBag.extra11 = "mandatory11";
            }
            if (serv.field12IsActive == true && serv.field12IsMandatory == true)
            {
                ViewBag.extra12 = "mandatory12";
            }

            Req_User agr = AgreementDetails((int)agID);
            agr.Agreement = ag;
            agr.serviceImage = servImg;

            if(agr.Agreement.price == null)
            {
                agr.Agreement.price = agr.Agreement.Service.price;
            }


            DynamicFieldAlternative DynField = db.DynamicFieldAlternatives.Where(z => z.ID == ag.Service.Availability1).FirstOrDefault();
            if (DynField.label == "Event")
            {
                ViewBag.Event = true;
            }
            else
            {
                ViewBag.Event = false;
            }

            if (!origin)
            {
                return View(agr);
            }
            else
            {
                return View("PartnerSiteBooking", agr);
            }
        }

        //Disclaimer: Currently not in use 
        //Presents a confirmation view before proceding to create the request
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm([Bind(Include = "ID,flightNr,contactName,contactEmail, notes, contactPhone, eventDate, eventTime, nrPersons, pickupLocation, price, reference, dropoffLocation,returnFlight,returnDate, returnTime, returnPickup, returnDropoff, extraDate1, extraDate2, extraDate3, extraTime1, extraTime2, extraTime3, extraText1, extraText2, extraText3, extraMultiText1, extraMultiText2, extraMultiText3")] Request request)
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }



            int agID = (int)Session["AgreementID"];
            var ag = db.Agreements.Where(z => z.ID == agID).FirstOrDefault();
            var serv = db.Services.Where(x => x.ID == ag.serviceID).FirstOrDefault();
            var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
   
            if (servType.hasReturn == true)
            {
                ViewBag.hasreturn = "return";
            }

            if (servType.isTransfer != true)
            {
                ViewBag.NotTransfer = "notTransfer";
            }


            
            Req_User agr = new Req_User { Agreement = ag, Request = request };
            return View(agr);
        }

        // Creates a request in the DB with the fields received through post
        // It also sends e-mail to operator notifying about the request
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,flightNr,contactName, contactEmail, notes, contactPhone, eventDate, eventTime, nrPersons, pickupLocation, price,  reference, dropoffLocation, returnFlight,returnDate, returnTime, returnPickup, returnDropoff, extraDate1, extraDate2, extraDate3, extraTime1, extraTime2, extraTime3, extraText1, extraText2, extraText3, extraMultiText1, extraMultiText2, extraMultiText3, istransfer, hasreturn, eventID, ClientNotes")] Request request, bool? FromPartnerSite)
        {

            if (Session["admin"] == null && Session["userID"] == null && FromPartnerSite == false)
            {
                return RedirectToAction("Index", "Login");
            }

            if (FromPartnerSite == false || FromPartnerSite == null)
            {
                request.bookerId = (int)Session["userID"];
            }

            request.agreementID = (int)Session["AgreementID"];
            request.bookDate = System.DateTime.Now;

            DateTime midnigth = new DateTime(request.bookDate.Value.Year, request.bookDate.Value.Month, request.bookDate.Value.Day, 0, 0, 0);

            request.bookTime = request.bookDate - midnigth;

            var part = new CityAccess.Partner();
            if (FromPartnerSite == false || FromPartnerSite == null)
            {
                part = db.Partners.Find((int)Session["partnerID"]);
            }
            var ag = db.Agreements.Where(z => z.ID == request.agreementID).FirstOrDefault();
            var serv = db.Services.Where(p => p.ID == ag.serviceID).FirstOrDefault();
            var opr = db.Partners.Where(a => a.ID == serv.operatorID).FirstOrDefault();


            if(ag.needsApproval == true)
            {
                request.stateID = "Submitted";
            }
            else
            {
                request.stateID = "Approved";
            }

            if (FromPartnerSite == false || FromPartnerSite == null)
            {

                try
                {
                    db.Requests.Add(request);
                    db.SaveChanges();

                    Request req = db.Requests.OrderByDescending(p => p.ID).First();
                    int requestID = req.ID;

                    RequestLog reqLog = new RequestLog();
                    reqLog.Date = System.DateTime.Now;
                    reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                    reqLog.requestID = requestID;
                    reqLog.userID = (int)Session["userID"];
                    reqLog.notes = "Booking created!";
                    db.RequestLogs.Add(reqLog);

                    db.SaveChanges();
                    //Set up and forwarding e-mail to operator
                    string og = " " + request.eventDate.ToString("dd-MM-yyyy");
                    string date = og.Replace("12:00:00 AM", " ");




                    EmailController email = new EmailController();
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                    string url = "ServicesRequested/Details/" + request.ID;
                    url = link + url;

                    string subject = string.Empty;

                    if (ag.needsApproval == true)
                    {
                        subject = "New request#" + request.ID + " " + ag.label + "-" + part.tradeName + "-" + date + "-" + request.nrPersons + "-needs approval";
                    }
                    else
                    {
                        subject = "New request#" + request.ID + " " + ag.label + "-" + part.tradeName + "-" + date + "-" + request.nrPersons + "-approved";
                    }

                    string content = "New request from " + part.tradeName + ":<br> ";

                    if (req.returnDate == null)
                    {

                        content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                            "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.label + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                            + "<tr><td> Agent </td><td> " + ag.Partner.tradeName + " </td></tr>"
                            + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                            "<tr><td>Time</td><td>" + request.eventTime + "</td></tr>" +
                            "<tr><td>Client name</td><td>" + request.contactName + "</td></tr>" +
                            "<tr><td>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                            "<tr><td>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                            "<tr><td>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                            "<tr><td>Price</td><td>" + request.price + "</td></tr>";
                        if (request.pickupLocation != null)
                        {
                            content = content +
                            "<tr><td>Pick up location</td><td>" + request.pickupLocation + "</td></tr>";
                        }
                        if (request.dropoffLocation != null)
                        {
                            content = content +
                            "<tr><td>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>";
                        }
                        if (request.flightNr != null)
                        {
                            content = content +
                            "<tr><td>Flight number</td><td>" + request.flightNr + "</td></tr>";
                        }
                        content = content +
                            "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                            "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                    }
                    else
                    {
                        //og = " " + req.returnDate;
                        string returnDate = " " + request.returnDate?.ToString("dd-MM-yyyy");

                        content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                             + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name + "</td></tr>"
                             + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                             + "<tr><td> Agent </td><td> " + ag.Partner.tradeName + " </td></tr>"
                             + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                             "<tr><td>Time</td><td>" + request.eventTime + "</td></tr>" +
                             "<tr><td>Client name</td><td>" + request.contactName + "</td></tr>" +
                             "<tr><td>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                             "<tr><td>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                             "<tr><td>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                             "<tr><td>Price</td><td>" + request.price + "</td></tr>" +
                             "<tr><td>Pick up location</td><td>" + request.pickupLocation + "</td></tr>" +
                             "<tr><td>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>" +
                             "<tr><td>Flight number</td><td>" + request.flightNr + "</td></tr>" +
                             "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                             "<tr><td>Return Time</td><td>" + request.returnTime + "</td></tr>" +
                             "<tr><td>Return flight number</td><td>" + request.returnFlight + "</td></tr>" +
                             "<tr><td>Return pickup</td><td>" + request.returnPickup + "</td></tr>" +
                             "<tr><td>Return dropoff</td><td>" + request.returnDropoff + "</td></tr>" +
                             "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                             "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                    }
                    email.Email_to_send(opr.email, url, content, subject);

                }
                catch (DbEntityValidationException dbEx)
                {
                    String error = string.Empty;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                            error = error + "Property:" + validationError.PropertyName + " Error:" + validationError.ErrorMessage;
                        }
                    }

                    ErrorController errorlog = new ErrorController();

                    String url = "Booking/Create";

                    //errorlog.ErrorLog((int)Session["userID"], error, url, request.bookDate, request.bookTime);

                    int agID = (int)Session["AgreementID"];
                    Req_User agr = AgreementDetails(agID);
                    agr.Agreement = ag;
                    var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
                    var servImg = db.ServiceImages.Where(j => j.serviceID == serv.ID && j.sequenceNR == 1).FirstOrDefault();
                    agr.serviceImage = servImg;
                    agr.Request = request;

                    if (agr.Agreement.price == null)
                    {
                        agr.Agreement.price = agr.Agreement.Service.price;
                    }
                    if (servType.hasReturn == true)
                    {
                        ViewBag.Control = "return";
                    }

                    if (servType.isTransfer != true)
                    {
                        ViewBag.NotTransfer = "notTransfer";
                    }

                    return View("Index", agr);
                }
            }
            else
            {
                try {

                request.bookerId = 1;
                request.stateID = "Site Approval";
                db.Requests.Add(request);
                db.SaveChanges();

                Request req = db.Requests.OrderByDescending(p => p.ID).First();
                int requestID = req.ID;

                RequestLog reqLog = new RequestLog();
                reqLog.Date = System.DateTime.Now;
                reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                reqLog.requestID = requestID;
                reqLog.notes = "Booking created by site request!";
                db.RequestLogs.Add(reqLog);

                db.SaveChanges();

                //Set up and forwarding e-mail to operator
                string og = " " + request.eventDate.ToString("dd-MM-yyyy");
                string date = og.Replace("12:00:00 AM", " ");

                var agent = db.Partners.Where(a => a.ID == ag.partnerID).FirstOrDefault(); 

                EmailController email = new EmailController();
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                string url = "ServicesRequested/Details/" + request.ID;
                url = link + url;

                string subject = string.Empty;

                if (ag.needsApproval == true)
                {
                    subject = "New request#" + request.ID + " " + ag.label + "-from client "+request.contactName+"-" + date + "-" + request.nrPersons + "-needs approval";
                }
                else
                {
                    subject = "New request#" + request.ID + " " + ag.label + "-from client"+request.contactName+"-" + date + "-" + request.nrPersons + "-approved";
                }

                string content = "New request from " + request.contactName + ":<br> ";

                if (req.returnDate == null)
                {

                    content = content + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                        "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + ag.label + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + ag.Partner.tradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.eventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.contactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.price + "</td></tr>";
                    if (request.pickupLocation != null)
                    {
                        content = content +
                        "<tr><td>Pick up location</td><td>" + request.pickupLocation + "</td></tr>";
                    }
                    if (request.dropoffLocation != null)
                    {
                        content = content +
                        "<tr><td>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>";
                    }
                    if (request.flightNr != null)
                    {
                        content = content +
                        "<tr><td>Flight number</td><td>" + request.flightNr + "</td></tr>";
                    }
                    content = content +
                        "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                else
                {
                    //og = " " + req.returnDate;
                    string returnDate = " " + request.returnDate?.ToString("dd-MM-yyyy");

                    content = content + "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                         + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name + "</td></tr>"
                         + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                         + "<tr><td> Agent </td><td> " + ag.Partner.tradeName + " </td></tr>"
                         + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                         "<tr><td>Time</td><td>" + request.eventTime + "</td></tr>" +
                         "<tr><td>Client name</td><td>" + request.contactName + "</td></tr>" +
                         "<tr><td>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                         "<tr><td>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                         "<tr><td>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                         "<tr><td>Price</td><td>" + request.price + "</td></tr>" +
                         "<tr><td>Pick up location</td><td>" + request.pickupLocation + "</td></tr>" +
                         "<tr><td>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>" +
                         "<tr><td>Flight number</td><td>" + request.flightNr + "</td></tr>" +
                         "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                         "<tr><td>Return Time</td><td>" + request.returnTime + "</td></tr>" +
                         "<tr><td>Return flight number</td><td>" + request.returnFlight + "</td></tr>" +
                         "<tr><td>Return pickup</td><td>" + request.returnPickup + "</td></tr>" +
                         "<tr><td>Return dropoff</td><td>" + request.returnDropoff + "</td></tr>" +
                         "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                         "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                }
                email.Email_to_send(agent.email, url, content, subject);

            }
                catch (DbEntityValidationException dbEx)
            {
                String error = string.Empty;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        error = error + "Property:" + validationError.PropertyName + " Error:" + validationError.ErrorMessage;
                    }
                }

                ErrorController errorlog = new ErrorController();

                String url = "Booking/Create";

                //errorlog.ErrorLog((int)Session["userID"], error, url, request.bookDate, request.bookTime);

                int agID = request.agreementID;
                Req_User agr = AgreementDetails(agID);
                agr.Agreement = ag;
                var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
                var servImg = db.ServiceImages.Where(j => j.serviceID == serv.ID && j.sequenceNR == 1).FirstOrDefault();
                agr.serviceImage = servImg;
                agr.Request = request;

                if (agr.Agreement.price == null)
                {
                    agr.Agreement.price = agr.Agreement.Service.price;
                }
                if (servType.hasReturn == true)
                {
                    ViewBag.Control = "return";
                }

                if (servType.isTransfer != true)
                {
                    ViewBag.NotTransfer = "notTransfer";
                }
                return View("Index", agr);
            }
            return View("BookingSuccessfull");
        }
            return RedirectToAction("Index", "ServicesBooked");
    }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="servID"></param>
        /// <returns></returns>
        public ActionResult Event(int servID)
        {
            var events = db.Events.Where(x => x.serviceID == servID && x.eventDate >= System.DateTime.Today).OrderBy(a => a.eventDate + " " + a.startTime);
            IQueryable<Request> queryable = null;
            var reqs = queryable;
            int booked = 0;
            foreach (var e in events)
            {
                reqs = db.Requests.Where(a => a.eventID == e.ID && a.stateID != "Canceled");
                
                foreach (var i in reqs)
                {
                    booked = booked + (int)i.nrPersons;
                }
                e.booked = booked;
                e.spaces = e.maxPersons - booked;

                booked = 0;
            }
            MultipleEvent mltEvent = new MultipleEvent();

            //Following the MVC ideology which states that default values are set in the controller
            mltEvent.Events = events.ToList();
            mltEvent.serviceID = servID;
            mltEvent.startDate = System.DateTime.Today;
            mltEvent.endDate = System.DateTime.Today;

            ViewBag.stateID = db.EventStates;

            return View(mltEvent);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Event([Bind(Include = "startDate, endDate, serviceID")]Event ev)
        {
            IQueryable<Event> events = null;

            if(ev.endDate >= ev.startDate)
            {
                 events = db.Events.Where(x => x.serviceID == ev.serviceID && x.eventDate >= ev.startDate && x.eventDate <= ev.endDate).OrderBy(a => a.eventDate + " " + a.startTime);
            }
            
            if(ev.startDate > ev.endDate)
            {
                events = db.Events.Where(x => x.serviceID == ev.serviceID && x.eventDate >= ev.startDate).OrderBy(a => a.eventDate + " " + a.startTime);
            }
            
            IQueryable<Request> queryable = null;
            var reqs = queryable;
            int booked = 0;
            foreach (var e in events)
            {
                reqs = db.Requests.Where(a => a.eventID == e.ID && a.stateID != "Canceled");

                foreach (var i in reqs)
                {
                    booked = booked + (int)i.nrPersons;
                }
                e.booked = booked;
                e.spaces = e.maxPersons - booked;

                booked = 0;
            }
            MultipleEvent mltEvent = new MultipleEvent();

            //Following the MVC ideology which states that default values are set in the controller
            mltEvent.Events = events.ToList();
            mltEvent.serviceID = ev.serviceID;
            mltEvent.startDate = ev.startDate;
            mltEvent.endDate = ev.endDate;

            ViewBag.stateID = db.EventStates;

            return View(mltEvent);
        }

        //Checks if the agreement fields are populated
        //If yes - fetches their values 
        //If no - fetches the same field values from service
        //
        // Created to aid in presenting agreement details
        [NonAction]
        public Req_User AgreementDetails(int agID)
        {
            Agreement ag = db.Agreements.Find(agID);
            Service serv = ag.Service;
            DynamicFieldAlternative dFA = new DynamicFieldAlternative();

            Req_User req_User = new Req_User {
                Description = ag.description,
                CommissionValue = ag.commissionValue,
                AgentInstructions = ag.agentInstructions,
                ConfirmationText = ag.messageTemplate,
                CancellationPolicy = ag.cancellationPolicy,
                PriceValue = ag.price,
                AgentPaymentValue = ag.PaymentAgent,
                AgName = ag.label
            };


            if (ag.priceType == null)
            {
                dFA = db.DynamicFieldAlternatives.Find(serv.priceType); 
                req_User.PriceType = dFA.label;
            }
            else
            {
                dFA = db.DynamicFieldAlternatives.Find(ag.priceType);
                req_User.PriceType = dFA.label;
            }
            if (ag.TypeCommission == null)
            {
                dFA = db.DynamicFieldAlternatives.Find(serv.comissionType);
                req_User.CommissionType = dFA.label;
            }
            else
            {
                dFA = db.DynamicFieldAlternatives.Find(ag.TypeCommission);
                req_User.CommissionType = dFA.label;
            }
            if (ag.PaymentAgentType == null)
            {
                dFA = db.DynamicFieldAlternatives.Find(serv.PaymentAgentType);
                req_User.AgentPaymentType = dFA.label;
            }
            else
            {
                dFA = db.DynamicFieldAlternatives.Find(ag.PaymentAgentType);
                req_User.AgentPaymentType = dFA.label;
            }
            if (ag.description == null)
            {
                req_User.Description= serv.description;
            }
            if (ag.commissionValue == null)
            {
                req_User.CommissionValue = serv.commissionValue;
            }
            if (ag.PaymentAgent == null)
            {
                req_User.AgentPaymentValue = serv.PaymentAgent;
            }
            if (ag.price == null)
            {
                req_User.PriceValue = serv.price;
            }
            if (ag.agentInstructions == null)
            {
                req_User.AgentInstructions = serv.agentInstructions;
            }
            if (ag.messageTemplate == null)
            {
                req_User.ConfirmationText = serv.ConfirmationText;
            }
            if (ag.cancellationPolicy == null)
            {
                req_User.CancellationPolicy = serv.cancellationPolicy;
            }
            if(ag.label == null)
            {
                req_User.AgName = serv.name;
            }

            return req_User;
        }



    }
}