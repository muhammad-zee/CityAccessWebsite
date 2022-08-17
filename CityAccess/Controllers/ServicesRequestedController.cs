
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Net.Mail;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text;

namespace CityAccess.Controllers
{
    public class ServicesRequestedController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();



        // Function that returns a Json with query results according to the filters received on input
        // Inputs: Status - request status, Agent - request agent, ServTitle - service title
        //
        //Originally created to fill calendar view
        public JsonResult Active(string Status, string Agent, string ServTitle)
        {

            int partnerID = (int)Session["partnerID"];


            var requests1 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID
                  join partn in db.Partners on ag.Partner.ID equals partn.ID
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()
                  select
                        new Req_forTransfer
                        {
                            ID = requ.ID,
                            AgreementID = requ.agreementID,
                            BookerId = requ.bookerId,
                            ContactName = requ.contactName,
                            ContactEmail = requ.contactEmail,
                            ContactPhone = requ.contactPhone,
                            EventDate = requ.eventDate,
                            EventTime = requ.eventTime,
                            NrPersons = requ.nrPersons,
                            Price = requ.price,
                            Reference = requ.reference,
                            Notes = requ.notes,
                            PickupLocation = requ.pickupLocation,
                            DropoffLocation = requ.dropoffLocation,
                            FlightNr = requ.flightNr,
                            StateID = requ.stateID,
                            BookingDate = requ.bookDate,
                            OperatorNotes = requ.OperatorNotes,
                            AgOp = requ.Agreement.Partner.tradeName,
                            Aglabel = requ.Agreement.label,
                            Servlabel = serv.name,
                            operatorID = requ.Agreement.Service.operatorID,
                            logo = requ.Agreement.Partner.PartnerLogoes.FirstOrDefault().Image,
                            Duration = serv.Duration, 
                            eventID = requ.eventID,
                            EventNotes = ev.notes,
                            EventStatus = ev.stateID,
                            Leg = 1,
                            HasReturn = serTyp.hasReturn,
                            PartnerID = requ.Agreement.partnerID
                        };

            if (Agent != "")
            {
                requests1 = requests1.Where(a => a.AgOp == Agent);
            }
            if (ServTitle != "")
            {
                requests1 = requests1.Where(a => a.Servlabel == ServTitle);
            }
            if (Status == "")
            {
                requests1 = requests1.Where(b => b.StateID == "Submitted" || b.StateID == "Approved");
                requests1 = requests1.Where(a => a.EventDate >= System.DateTime.Today);
            }
            else
            {
                if (Status != "All")
                {
                    requests1 = requests1.Where(b => b.StateID == Status);
                }
                else
                {
                    requests1 = requests1.Where(b => b.StateID != "Site Approval" && b.StateID != "Site Canceled");
                }
            }
            requests1 = requests1.Where(a => a.operatorID == partnerID);
            requests1 = requests1.OrderBy(a => a.EventDate + " " + a.EventTime);
            

            var requests2 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID
                  join partn in db.Partners on ag.Partner.ID equals partn.ID
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()
                  select
                       new Req_forTransfer
                       {
                           ID = requ.ID,
                           AgreementID = requ.agreementID,
                           BookerId = requ.bookerId,
                           ContactName = requ.contactName,
                           ContactEmail = requ.contactEmail,
                           ContactPhone = requ.contactPhone,
                           EventDate = requ.returnDate,
                           EventTime = requ.returnTime,
                           NrPersons = requ.nrPersons,
                           Price = new decimal(),
                           Reference = requ.reference,
                           Notes = requ.notes,
                           PickupLocation = requ.returnPickup,
                           DropoffLocation = requ.returnDropoff,
                           FlightNr = requ.returnFlight,
                           StateID = requ.stateID,
                           BookingDate = requ.bookDate,
                           OperatorNotes = requ.OperatorNotes,
                           AgOp = requ.Agreement.Partner.tradeName,
                           Aglabel = requ.Agreement.label,
                           Servlabel = serv.name,
                           operatorID = requ.Agreement.Service.operatorID,
                           logo = requ.Agreement.Partner.PartnerLogoes.FirstOrDefault().Image,
                           Duration = serv.Duration,
                           eventID = requ.eventID,
                           EventNotes = ev.notes,
                           EventStatus = ev.stateID,
                           Leg = 2 ,
                           HasReturn = serTyp.hasReturn,
                           PartnerID = requ.Agreement.partnerID
                       };

            requests2 = requests2.Where(a => a.EventDate != null);

            if (Agent != "")
            {
                requests2 = requests2.Where(a => a.AgOp == Agent);
            }
            if (ServTitle != "")
            {
                requests2 = requests2.Where(a => a.Servlabel == ServTitle);
            }
            if (Status == "")
            {
                requests2 = requests2.Where(b => b.StateID == "Submitted" || b.StateID == "Approved");
                requests2 = requests2.Where(a => a.EventDate >= System.DateTime.Today);
            }
            else
            {
                if (Status != "All")
                {
                    requests2 = requests2.Where(b => b.StateID == Status);
                }
                else
                {
                    requests2 = requests2.Where(b => b.StateID != "Site Approval" && b.StateID != "Site Canceled");
                }
            }
            requests2 = requests2.Where(a => a.operatorID == partnerID);
            requests2 = requests2.OrderBy(a => a.EventDate + " " + a.EventTime);

            var requests = requests1.Union(requests2).OrderBy(x => x.eventID);

            List<CityAccess.Req_forTransfer> Req_List = requests.ToList();

            int eventID = 0;
            int index = 0;
            string partners = string.Empty;
            string clientName = string.Empty;
            int nrPersons = 0;
            string status = string.Empty;
            string notes = string.Empty;
            bool present = false;

            int limit = Req_List.Count;


            int[] parts = new int[limit]; //size is limit, because it is the maximum posssible number of partners, a different one per request

            for (int i = 0; i < Req_List.Count; i++)
            {
                if (Req_List[i].eventID != null)
                {
                    if (Req_List[i].eventID != eventID)
                    {

                        if (eventID != 0)
                        {
                            Req_List[index].ID = eventID;
                            Req_List[index].AgOp = partners;
                            Req_List[index].ContactName = clientName;
                            Req_List[index].NrPersons = nrPersons;
                            Req_List[index].StateID = status;
                            Req_List[index].Notes = notes;
                        }

                        eventID = (int)Req_List[i].eventID.Value;
                        index = i;

                        parts = new int[limit];

                        parts[i] = (int)Req_List[i].PartnerID;

                        partners = Req_List[i].AgOp;
                        clientName = Req_List[i].ContactName;
                        nrPersons = (int)Req_List[i].NrPersons;
                        status = Req_List[i].EventStatus;
                        notes = Req_List[i].EventNotes;
                    }
                    else
                    {
                        int IDatual = (int)Req_List[i].PartnerID;

                        for (int j = 0; j < limit; j++)
                        {
                            if (parts[j] == IDatual)
                            {
                                present = true;
                                break;
                            }
                            else
                            {
                                present = false;
                            }
                        }

                        if (present == false)
                        {
                            partners = partners + ", " + Req_List[i].AgOp;
                        }

                        parts[i] = (int)Req_List[i].PartnerID;

                        clientName = clientName + ", " + Req_List[i].ContactName;
                        nrPersons = nrPersons + (int)Req_List[i].NrPersons;

                        Req_List.RemoveAt(i);
                        i--;

 
                    }
                }
            }

            Req_List[index].ID = eventID;
            Req_List[index].AgOp = partners;
            Req_List[index].ContactName = clientName;
            Req_List[index].NrPersons = nrPersons;
            Req_List[index].StateID = status;
            Req_List[index].Notes = notes;

            return new JsonResult { Data = Req_List.OrderBy(x => x.EventDate + " " + x.EventTime), JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
            
        }



        // Returns a BookingFilters class to populate the active services requested page of an agent 
        // Inputs: int c - is not null when callendar view is requested, then proceeds to call callendar view
        //
        // GET: ServicesRequested
        public ActionResult Index(int? c)
        {
            int partnerID;

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
                partnerID = (int)Session["partnerID"];
            }
            else
            {
                Partner partner = db.Partners.FirstOrDefault();
                partnerID = partner.ID;
            }

            var requests1 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()

                  join partn in db.Partners on us.partnerId equals partn.ID
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  where serv.operatorID == partnerID && (requ.stateID == "Submitted" || requ.stateID == "Approved") && (requ.eventDate >= System.DateTime.Today)
                  orderby requ.eventDate, requ.eventTime
                  select new Req_Partner {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.ID,
                          AgreementID = requ.agreementID,
                          BookerId = requ.bookerId,
                          ContactName = requ.contactName,
                          ContactEmail = requ.contactEmail,
                          ContactPhone = requ.contactPhone,
                          EventDate = requ.eventDate,
                          EventTime = requ.eventTime,
                          NrPersons = requ.nrPersons,
                          Price = requ.price,
                          Reference = requ.reference,
                          Notes = requ.notes,
                          PickupLocation = requ.pickupLocation,
                          DropoffLocation = requ.dropoffLocation,
                          FlightNr = requ.flightNr,
                          StateID = requ.stateID,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.eventID,
                          EventNotes = ev.notes,
                          Leg = 1
                      }, Partner = partn, Agreement = ag, ServiceType = serTyp };

            var requests2 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()

                  join partn in db.Partners on us.partnerId equals partn.ID
                  into part1
                  from partn in part1.DefaultIfEmpty()
                  where serv.operatorID == partnerID && (requ.stateID == "Submitted" || requ.stateID == "Approved") && (requ.returnDate != null && requ.returnDate >= System.DateTime.Today)
                  orderby requ.eventDate, requ.eventTime
                  select new Req_Partner
                  {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.ID,
                          AgreementID = requ.agreementID,
                          BookerId = requ.bookerId,
                          ContactName = requ.contactName,
                          ContactEmail = requ.contactEmail,
                          ContactPhone = requ.contactPhone,
                          EventDate = requ.returnDate,
                          EventTime = requ.returnTime,
                          NrPersons = requ.nrPersons,
                          Price = new decimal(),
                          Reference = requ.reference,
                          Notes = requ.notes,
                          PickupLocation = requ.returnPickup,
                          DropoffLocation = requ.returnDropoff,
                          FlightNr = requ.returnFlight,
                          StateID = requ.stateID,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.eventID,
                          EventNotes = ev.notes,
                          Leg = 2
                      },
                      Partner = partn,
                      Agreement = ag,
                      ServiceType = serTyp
                  };

            var requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.eventID).OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

            List<Req_Partner> reqs = requests.ToList();
            decimal TotalPrice= 0;
            int eventID = 0;
            int TotalPersons = 0;
            int limit = reqs.Count();

            foreach (var r in requests)
            {

                if (r.req_ForTransfer.eventID != null)
                {

                    if(r.req_ForTransfer.eventID != eventID)
                    {
                        if(eventID != 0)
                        {
                            for(int i= 0; i < limit; i++)
                            {
                                if(reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
                                {
                                    reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
                                    reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
                                }
                            }
                        }

                        eventID = (int)r.req_ForTransfer.eventID;
                        TotalPrice = r.req_ForTransfer.Price;
                        TotalPersons = (int)r.req_ForTransfer.NrPersons;
                    }
                    else
                    {
                        TotalPrice = TotalPrice + r.req_ForTransfer.Price;
                        TotalPersons = TotalPersons + (int)r.req_ForTransfer.NrPersons;
                    }
                }
            }
            for (int i = 0; i < limit; i++)
            {
                if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
                {
                    reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
                    reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
                }
            }

            ViewBag.stateID = db.States.Where(x => x.ID != "Site Approval" && x.ID != "Site Canceled"); //Good for the time being, but not scalable, think of a better solution
            ViewBag.partners = db.Partners;

            ViewBag.agent = (from ag in db.Agreements
                                 join serv in db.Services on ag.serviceID equals serv.ID
                                 join part in db.Partners on ag.partnerID equals part.ID
                                 where partnerID == serv.operatorID
                                 select part).Distinct();

            ViewBag.Service = (from serv in db.Services
                               where partnerID == serv.operatorID
                               select serv).Distinct();

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
            items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
            items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
            items.Add(new SelectListItem() { Text = "All", Value = "All" });


            BookingFilters bookingFilters = new BookingFilters { Req_PartnerList = reqs /*requests.ToList()*/, StatusFilters = items, Status = string.Empty };

            if(c == 1)
            {
                return View("~/Views/ServicesRequested/Calendar.cshtml", bookingFilters);
            }
            else
            {
                return View(bookingFilters);
            }
        }


        //Querys the DB with filters received through post and then passes results to view
        //If Dwn variable has value the function flushes an excel with current view results
        //Input: filters selected to query
        //
        // Post: ServicesReqeuested
        [HttpPost]
        public ActionResult Index([Bind(Include = "ID, Notes, StateID ")] Req_forTransfer request, [Bind(Include = "Status, Partner, Req_PartnerList, Dwn,  Agent, ServTitle, Date, BookingDate, Date2, BookingDate2")] BookingFilters bookingFilters)
        {

            //System.Diagnostics.Debug.WriteLine("estou aqui");
            db.Configuration.LazyLoadingEnabled = false;

            db.Dispose();
            db = new CityAccessEntities();


            Partner part;
            User user = db.Users.Find((int)Session["userID"]); //For the e-mail
            int partnerID;

            if (Session["admin"] == null)
            {
               // needed for the database fetch
                partnerID = (int)Session["partnerID"];

              //  DB data needed for the e-mail
                part = db.Partners.Find(partnerID);


            }
            else
            {
                part = db.Partners.Where(a => a.tradeName == bookingFilters.Partner).FirstOrDefault();
                partnerID = part.ID;
            }

            if (bookingFilters == null && request == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            if (request.ID != 0)
            {
                Request req = db.Requests.Find(request.ID);
                var ag = db.Agreements.Where(z => z.ID == req.agreementID).FirstOrDefault();
                var serv = db.Services.Where(p => p.ID == ag.serviceID).FirstOrDefault();
                if (request.Notes != null)
                {
                    request.StateID = request.Notes;
                }

                string Changes = "-Status  from " + req.stateID + " to " + request.StateID + "\n";
                request.Notes = req.stateID;
                req.stateID = request.StateID;

                //DB data needed for the e-mail
                var agentUser = db.Users.Where(x => x.ID == req.bookerId).Include(x => x.Partner).FirstOrDefault();

                try
                {
                    db.Entry(req).State = EntityState.Modified;


                    RequestLog reqLog = new RequestLog();
                    reqLog.Date = System.DateTime.Now;
                    reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                    reqLog.requestID = req.ID;
                    reqLog.userID = (int)Session["userID"];
                    reqLog.notes = Changes;
                    db.RequestLogs.Add(reqLog);
                    db.SaveChanges();


                    //general info about the request
                    string reqDetails = String.Empty;

                    string og = " " + req.eventDate.ToString("dd-MM-yyyy");
                    string date = og.Replace("12:00:00 AM", " ");

                    Changes = "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                        + "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody><tr><td>Status</td><td>" + request.Notes + "</td><td>" + req.stateID + "</td></tr></tbody></table>";

                    reqDetails = "<br/><br><p></p>Request general details:<br/><br> ";

                    if (req.returnDate == null)
                    {
                       
                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                            + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name+"</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.tradeName + " </td></tr>" 
                            +"<tr><td> Agent </td><td> " + agentUser.Partner.tradeName + " </td></tr>" 
                            +"<tr><td> Date </td><td> "+og+" </td></tr>"+
                            "<tr><td>Time</td><td>" + req.eventTime + "</td></tr>" +
                            "<tr><td>Client name</td><td>" + req.contactName + "</td></tr>" +
                            "<tr><td>Client e-mail</td><td>" + req.contactEmail + "</td></tr>"+
                            "<tr><td>Client phone</td><td>" + req.contactPhone + "</td></tr>" +
                            "<tr><td>Nº of persons</td><td>" + req.nrPersons + "</td></tr>"+
                            "<tr><td>Price</td><td>" + req.price + "</td></tr>";
                        if (req.pickupLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Pick up location</td><td>" + req.pickupLocation + "</td></tr>";
                        }
                        if (req.dropoffLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Dropoff location</td><td>" + req.dropoffLocation + "</td></tr>";
                        }
                        if (req.flightNr != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Flight number</td><td>" + req.flightNr + "</td></tr>";
                        }

                        reqDetails = reqDetails +
                            "<tr><td>Client Notes</td><td>" + req.ClientNotes + "</td></tr>"+
                            "<tr><td>Notes</td><td>" + req.notes + "</td></tr>"+
                            "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                    }
                    else
                    {
                        //og = " " + req.returnDate;
                        string returnDate = " " + req.returnDate?.ToString("dd-MM-yyyy");

                       reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                            + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name+"</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.tradeName + " </td></tr>"
                            + "<tr><td> Agent </td><td> " + agentUser.Partner.tradeName + " </td></tr>"
                            + "<tr><td> Date </td><td> "+og+" </td></tr>"+
                            "<tr><td>Time</td><td>" + req.eventTime + "</td></tr>" +
                            "<tr><td>Client name</td><td>" + req.contactName + "</td></tr>" +
                            "<tr><td>Client e-mail</td><td>" + req.contactEmail + "</td></tr>"+
                            "<tr><td>Client phone</td><td>" + req.contactPhone + "</td></tr>" +
                            "<tr><td>Nº of persons</td><td>" + req.nrPersons + "</td></tr>"+
                            "<tr><td>Price</td><td>" + req.price + "</td></tr>"+
                            "<tr><td>Pick up location</td><td>" + req.pickupLocation + "</td></tr>"+
                            "<tr><td>Dropoff location</td><td>" + req.dropoffLocation + "</td></tr>"+
                            "<tr><td>Flight number</td><td>" + req.flightNr + "</td></tr>"+
                            "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                            "<tr><td>Return Time</td><td>" + req.returnTime + "</td></tr>" +
                            "<tr><td>Return flight number</td><td>" + req.returnFlight + "</td></tr>" +
                            "<tr><td>Return pickup</td><td>" + req.returnPickup + "</td></tr>" +
                            "<tr><td>Return dropoff</td><td>" + req.returnDropoff + "</td></tr>" +
                            "<tr><td>Client Notes</td><td>" + req.ClientNotes + "</td></tr>" +
                            "<tr><td>Notes</td><td>" + req.notes + "</td></tr>"+
                            "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                    }




                    //Set up and forwarding e-mails
                    string time = System.DateTime.Now.ToString("HH:mm");
                    EmailController email = new EmailController();
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                    string url = "ServicesBooked/Edit/" + req.ID;
                    url = link + url;

                    string subject = "Booking " + req.Agreement.label + "#" + req.ID + "-" + part.tradeName + " was changed."; 


                    Changes = "<br/><br>" + Changes;
                    string content = email.AgentContent(user.fullName, time) + Changes + reqDetails;

                    email.Email_to_send(agentUser.Partner.email, url, content, subject);

                    url = "ServicesRequested/Details/" + req.ID;
                    url = link + url;

                    subject = "Request " + req.Agreement.label + "#" + req.ID + "-" + agentUser.Partner.tradeName + " was changed.";

                    content = email.OperatorEditorContent(part.tradeName, time) + Changes + reqDetails;

                    email.Email_to_send(part.email, url, content, subject);

                    //content = email.EditorContent() + Changes;

                    //email.Email_to_send(user.email, url, content, subject);

                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "ServicesRequested/Index";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);

                    return RedirectToAction("Index", "Error");
                }
            }

            IQueryable<Req_Partner> queryable = null;
            var requests = queryable;

            var requests1 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()

                  join partn in db.Partners on ag.Partner.ID equals partn.ID
                  into part1 from partn in part1.DefaultIfEmpty()
                  select ( new Req_Partner
                  {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.ID,
                          AgreementID = requ.agreementID,
                          BookerId = requ.bookerId,
                          ContactName = requ.contactName,
                          ContactEmail = requ.contactEmail,
                          ContactPhone = requ.contactPhone,
                          EventDate = requ.eventDate,
                          EventTime = requ.eventTime,
                          NrPersons = requ.nrPersons,
                          Price = requ.price,
                          Reference = requ.reference,
                          Notes = requ.notes,
                          PickupLocation = requ.pickupLocation,
                          DropoffLocation = requ.dropoffLocation,
                          FlightNr = requ.flightNr,
                          StateID = requ.stateID,
                          BookingDate = requ.bookDate,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.eventID,
                          EventNotes = ev.notes,
                          Leg = 1
                      },
                      //Partner = partn != null ? partn : null,
                      Partner = partn,
                      Agreement = ag,
                      ServiceType = serTyp
                  });

            //filtering part
            if (bookingFilters.Agent != null)
            {
                requests1 = requests1.Where(a => a.Partner.tradeName == bookingFilters.Agent);
            }
            if (bookingFilters.ServTitle != null)
            {
                requests1 = requests1.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
            }
            if (bookingFilters.Date != null)
            {
                requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
            }
            if (bookingFilters.Date != null && bookingFilters.Date2 != null)
            {
                requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
            }
            if (bookingFilters.BookingDate != null)
            {
                requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
            }
            if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
            {
                requests1 = requests1.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
            }

            if (bookingFilters.Status == null)
            {
                requests1 = requests1.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
                requests1 = requests1.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
            }
            else
            {
                if (bookingFilters.Status != "All")
                {
                    requests1 = requests1.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
                }
                else
                {
                    requests1 = requests1.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
                }
            }
            requests1 = requests1.Where(a=> a.Agreement.Service.operatorID == partnerID);
            requests1 = requests1.OrderBy(a => a.req_ForTransfer.EventDate +" " + a.req_ForTransfer.EventTime);

            var requests2 =
                  from requ in db.Requests
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join us in db.Users on requ.bookerId equals us.ID
                  join serv in db.Services on ag.serviceID equals serv.ID
                  join serTyp in db.serviceTypes on serv.typeID equals serTyp.ID

                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()

                  join partn in db.Partners on ag.Partner.ID equals partn.ID
                  into part1 from partn in part1.DefaultIfEmpty()
                  select new Req_Partner
                  {
                      req_ForTransfer = new Req_forTransfer
                      {
                          ID = requ.ID,
                          AgreementID = requ.agreementID,
                          BookerId = requ.bookerId,
                          ContactName = requ.contactName,
                          ContactEmail = requ.contactEmail,
                          ContactPhone = requ.contactPhone,
                          EventDate = requ.returnDate,
                          EventTime = requ.returnTime,
                          NrPersons = requ.nrPersons,
                          Price = new decimal(),
                          Reference = requ.reference,
                          Notes = requ.notes,
                          PickupLocation = requ.returnPickup,
                          DropoffLocation = requ.returnDropoff,
                          FlightNr = requ.returnFlight,
                          StateID = requ.stateID,
                          BookingDate = requ.bookDate,
                          OperatorNotes = requ.OperatorNotes,
                          eventID = requ.eventID,
                          EventNotes = ev.notes, 
                          Leg = 2
                      },
                      //Partner = partn != null ? partn: null,
                      Partner = partn,
                      Agreement = ag,
                      ServiceType = serTyp
                  };

            //filtering part
            requests2 = requests2.Where(a => a.req_ForTransfer.EventDate != null);

            if (bookingFilters.Agent != null)
            {
                requests2 = requests2.Where(a => a.Partner.tradeName == bookingFilters.Agent);
            }
            if (bookingFilters.ServTitle != null)
            {
                requests2 = requests2.Where(a => a.Agreement.Service.name == bookingFilters.ServTitle);
            }
            if (bookingFilters.Date != null)
            {
                requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date);
            }
            if (bookingFilters.Date != null && bookingFilters.Date2 != null)
            {
                requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= bookingFilters.Date && a.req_ForTransfer.EventDate <= bookingFilters.Date2);
            }
            if (bookingFilters.BookingDate != null)
            {
                requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate);
            }
            if (bookingFilters.BookingDate != null && bookingFilters.BookingDate2 != null)
            {
                requests2 = requests2.Where(a => a.req_ForTransfer.BookingDate >= bookingFilters.BookingDate && a.req_ForTransfer.BookingDate <= bookingFilters.BookingDate2);
            }
            if (bookingFilters.Status == null)
            {
                requests2 = requests2.Where(b => b.req_ForTransfer.StateID == "Submitted" || b.req_ForTransfer.StateID == "Approved");
                requests2 = requests2.Where(a => a.req_ForTransfer.EventDate >= System.DateTime.Today);
            }
            else
            {
                if (bookingFilters.Status != "All")
                {
                    requests2 = requests2.Where(b => b.req_ForTransfer.StateID == bookingFilters.Status);
                }
                else
                {
                    requests2 = requests2.Where(b => b.req_ForTransfer.StateID != "Site Approval" && b.req_ForTransfer.StateID != "Site Canceled");
                }
            }
            requests2 = requests2.Where(a => a.Agreement.Service.operatorID == partnerID);
            requests2 = requests2.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

            requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.EventDate + " " + x.req_ForTransfer.EventTime);

            List<Req_Partner> reqs = requests.ToList();
            decimal TotalPrice = 0;
            int eventID = 0;
            int TotalPersons = 0;
            int limit = reqs.Count();

            foreach (var r in requests)
            {

                if (r.req_ForTransfer.eventID != null)
                {

                    if (r.req_ForTransfer.eventID != eventID)
                    {
                        if (eventID != 0)
                        {
                            for (int i = 0; i < limit; i++)
                            {
                                if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
                                {
                                    reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
                                    reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
                                }
                            }
                        }

                        eventID = (int)r.req_ForTransfer.eventID;
                        TotalPrice = r.req_ForTransfer.Price;
                        TotalPersons = (int)r.req_ForTransfer.NrPersons;
                    }
                    else
                    {
                        TotalPrice = TotalPrice + r.req_ForTransfer.Price;
                        TotalPersons = TotalPersons + (int)r.req_ForTransfer.NrPersons;
                    }
                }
            }
            for (int i = 0; i < limit; i++)
            {
                if (reqs.Skip(i).First().req_ForTransfer.eventID == eventID)
                {
                    reqs.Skip(i).First().req_ForTransfer.TotalPrice = TotalPrice;
                    reqs.Skip(i).First().req_ForTransfer.TotalNrPersons = TotalPersons;
                }
            }


            ViewBag.agent = (from ag in db.Agreements
                             join serv in db.Services on ag.serviceID equals serv.ID
                             join partn in db.Partners on ag.partnerID equals partn.ID
                             where partnerID == serv.operatorID
                             select partn).Distinct();

            ViewBag.Service = (from serv in db.Services
                               where partnerID == serv.operatorID
                               select serv).Distinct();




            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
            items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
            items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
            items.Add(new SelectListItem() { Text = "All", Value = "All" });

            bookingFilters.StatusFilters = items;
            bookingFilters.Req_PartnerList = requests.ToList();
            ViewBag.stateID = db.States.Where(x => x.ID != "Site Approval" && x.ID != "Site Canceled"); //Good for the time being, but not scalable, think of a better solution
            ViewBag.partners = db.Partners;


            //Excel part
            if (bookingFilters.Dwn == 1)
            {
                StringWriter sw = new StringWriter();
                var requestsToUs = new System.Data.DataTable("requests");
                requestsToUs.Columns.Add("Request", typeof(string));
                requestsToUs.Columns.Add("Service Name", typeof(string));
                requestsToUs.Columns.Add("Partner", typeof(string));
                requestsToUs.Columns.Add("Customer", typeof(string));
                requestsToUs.Columns.Add("Date", typeof(string));
                requestsToUs.Columns.Add("Time", typeof(TimeSpan));
                requestsToUs.Columns.Add("Pax", typeof(int));
                requestsToUs.Columns.Add("Status", typeof(string));
                requestsToUs.Columns.Add("Pick up Location", typeof(string));
                requestsToUs.Columns.Add("Flight number", typeof(string));
                requestsToUs.Columns.Add("Drop off Location", typeof(string));
                requestsToUs.Columns.Add("Price", typeof(string));
                requestsToUs.Columns.Add("Com.", typeof(string));
                requestsToUs.Columns.Add("Client Name", typeof(string));
                requestsToUs.Columns.Add("Notes", typeof(string));
                requestsToUs.Columns.Add("Operator Notes", typeof(string));


                string partnerTradename;

                foreach (var item in bookingFilters.Req_PartnerList)
                {
                    if(item.Partner == null)
                    {
                        partnerTradename = "General Request";
                    }
                    else
                    {
                        partnerTradename = item.Partner.tradeName;
                    }

                    requestsToUs.Rows.Add(
                        item.req_ForTransfer.ID,
                        item.Agreement.label,
                        partnerTradename,
                        item.req_ForTransfer.ContactName,
                        item.req_ForTransfer.EventDate.Value.ToString("dd/MM/yyyy"),
                        item.req_ForTransfer.EventTime,
                        item.req_ForTransfer.NrPersons,
                        item.req_ForTransfer.StateID,
                        item.req_ForTransfer.PickupLocation,
                        item.req_ForTransfer.FlightNr,
                        item.req_ForTransfer.DropoffLocation,
                        item.req_ForTransfer.Price.ToString().Replace(".", ","),
                        item.Agreement.commissionValue.ToString().Replace(".", ","),
                        item.req_ForTransfer.ContactName,
                        item.req_ForTransfer.Notes,
                        item.req_ForTransfer.OperatorNotes
                        );

                    System.Diagnostics.Debug.WriteLine(item.req_ForTransfer.Price.ToString().Replace(".", ","));
                }

                var grid = new GridView();
                grid.DataSource = requestsToUs;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachement;filename=ServicesRequested.xls");
                Response.ContentType = "application/ms-excel";

                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Write(sw.ToString());
                Response.Flush();
                Response.End();

            }



            BookingFilters bookingFilters1 = new BookingFilters { Req_PartnerList = reqs, StatusFilters = items, Status = bookingFilters.Status };

            if (request.Notes != null)
            {
                ModelState.Clear(); // temporary solution for state changes that change dropdown value 
            }
            return View(bookingFilters1);

        }


        // GET: ServicesRequested/Details/5
        //Querys the DB to pass service details to the view
        //
        //Input: int? id - the pretended request id , int? leg - indicates which leg of the requested is to be presented, if the request has 2 legs
        public ActionResult Details(int? id, int? leg)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
            return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }



            Request request = db.Requests.Find(id);


            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(request.Agreement.Service.Partner.ID);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }


            var reqLog_list = db.RequestLogs.Where(z => z.requestID == id);

            var ag = db.Agreements.Where(z => z.ID == request.agreementID).FirstOrDefault();
            var serv = db.Services.Where(x => x.ID == ag.serviceID).FirstOrDefault();
            var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
            var opr = db.Partners.Where(a => a.ID == serv.operatorID).FirstOrDefault();

            if (request == null)
            {
                return HttpNotFound();
            }
            if (servType.isTransfer != true)
            {
                ViewBag.NotTransfer = "notTransfer";
            }

            int partnerID = (int)Session["partnerID"];
            var users = db.Users.AsNoTracking().Where(x => x.partnerId == partnerID);

            ViewBag.stateID = db.States.Where(x => x.ID != "Site Approval" && x.ID != "Site Canceled"); //Good for the time being, but not scalable, think of a better solution
            ViewBag.leg = leg;
            ViewBag.ID = id;

            List<SelectListItem> ForResponsible = new SelectList(users, "ID", "fullName").ToList();

            ForResponsible.Insert(ForResponsible.Count, (new SelectListItem { Text = "   ", Value = "0" }));
            
            ViewBag.Users = ForResponsible;

            ReqLog reqLog = new ReqLog { Request = request, ReqLog_List = reqLog_list.ToList(), Agreement = ag, operatorNotes = request.OperatorNotes };

            return View(reqLog);
        }



        // GET: ServicesRequested/Details/5
        //
        //Updates the DB fields Operator Notes and Status, if changed
        //
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Details([Bind(Include = "ID, stateID, notes, OperatorNotes, contactEmail, eventDate, eventTime, ResponsibleId")] Request request, String stateID, String operatorNotes, String content1, String Subject, String Cc, String ResponsibleID)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            //DB data needed for the e-mail
            EmailController email = new EmailController();

            int? responsibleID = null;

            if(ResponsibleID != "")
            {
                responsibleID = Int32.Parse(ResponsibleID);
            }

            
            var part = db.Partners.Find((int)Session["partnerID"]);
            var user = db.Users.Find((int)Session["userID"]);

            if (request == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Request req = db.Requests.Find(request.ID);
            var ag = db.Agreements.Where(z => z.ID == req.agreementID).FirstOrDefault();
            var serv = db.Services.Where(p => p.ID == ag.serviceID).FirstOrDefault();

            var newResponsible = db.Users.Where(x => x.ID == responsibleID).FirstOrDefault();

            string Changes = string.Empty;

            string Changes1 = "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody>";
            if (req.stateID != stateID && stateID != "")
            {
                Changes = "-Status  from " + req.stateID + " to " + stateID + "\n";
                Changes1 = Changes1 + "<tr><td>Status</td><td>" + req.stateID + "</td><td>" + stateID + "</td></tr>";
                req.stateID = stateID;
            }
            if (req.OperatorNotes == null && operatorNotes != "")
            {
                Changes = Changes + "-Operator notes from no value to " + operatorNotes + "\n";
                Changes1 = Changes1 + "<tr><td>Operator notes</td><td>No value</td><td>" + operatorNotes + "</td></tr>";
                req.OperatorNotes = operatorNotes;
            }
            else
            {
                if (req.OperatorNotes != operatorNotes && operatorNotes != "")
                {
                    Changes = Changes + "-Operator notes from " + req.OperatorNotes + " to " + operatorNotes + "\n";
                    Changes1 = Changes1 + "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td><td>" + operatorNotes + "</td></tr>";
                    req.OperatorNotes = operatorNotes;
                }
            }
            if (req.ResponsibleId != responsibleID && (newResponsible != null || responsibleID == 0))
            {
                string oldResponsible = " no value ";

                string NewResponsible = "no value";

                if(req.User1 != null)
                {
                    oldResponsible = req.User1.fullName;
                }

                if(newResponsible != null)
                {
                    NewResponsible = newResponsible.fullName;
                }

                Changes = Changes + "- Assigned from " + oldResponsible + " to " + NewResponsible + "\n";
                Changes1 = Changes1 + "<tr><td>Assigned</td><td>" + oldResponsible + "</td><td>" + NewResponsible + "</td></tr>";

                if(responsibleID == 0)
                {
                    responsibleID = null;
                }

                req.ResponsibleId = responsibleID;
            }

            Changes1 = Changes1 + "</tbody></table>";
            
                //DB data needed for the e-mail
                var agentUser = db.Users.Where(x => x.ID == req.bookerId).Include(x => x.Partner).FirstOrDefault();

                try
                {
                    if(Changes != string.Empty)
                    { 
                        db.Entry(req).State = EntityState.Modified;

                        RequestLog reqLog = new RequestLog();
                        reqLog.Date = System.DateTime.Now;
                        reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                        reqLog.requestID = req.ID;
                        reqLog.userID = (int)Session["userID"];
                        reqLog.notes = Changes;
                        db.RequestLogs.Add(reqLog);
                        db.SaveChanges();

                        //Set up and forwarding e-mails

                        var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                        string url = "ServicesBooked/Edit/" + req.ID;
                        url = link + url;

                        string subject = "Booking " + req.Agreement.label + "#" + req.ID + "-" + part.tradeName + " was changed.";

                        Changes = "<br/><br>" + Changes1;


                        //general info about the request
                        string reqDetails = String.Empty;

                        string og = " " + req.eventDate.ToString("dd-MM-yyyy");
                        string date = og.Replace("12:00:00 AM", " ");

                        reqDetails = "<br/><br><p></p>Request general details:<br/><br> ";

                        if (req.returnDate == null)
                        {

                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                            + "<tr><th>Field</th><th>Value</th></tr></thead>"+
                            "<tbody style='line-height: 1.5;font-size: 12px;'>" +
                            "<tr><td>Service</td><td>" + serv.name + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.tradeName + " </td></tr>"
                            + "<tr><td> Agent </td><td> " + agentUser.Partner.tradeName + " </td></tr>"
                            + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                            "<tr><td>Time</td><td>" + req.eventTime + "</td></tr>" +
                            "<tr><td>Client name</td><td>" + req.contactName + "</td></tr>" +
                            "<tr><td>Client e-mail</td><td>" + req.contactEmail + "</td></tr>" +
                            "<tr><td>Client phone</td><td>" + req.contactPhone + "</td></tr>" +
                            "<tr><td>Nº of persons</td><td>" + req.nrPersons + "</td></tr>" +
                            "<tr><td>Price</td><td>" + req.price + "</td></tr>";
                        if (req.pickupLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Pick up location</td><td>" + req.pickupLocation + "</td></tr>";
                        }
                        if (req.dropoffLocation != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Dropoff location</td><td>" + req.dropoffLocation + "</td></tr>";
                        }
                        if (req.flightNr != null)
                        {
                            reqDetails = reqDetails +
                            "<tr><td>Flight number</td><td>" + req.flightNr + "</td></tr>";
                        }
                        reqDetails = reqDetails +
                            "<tr><td>Client notes</td><td>" + req.ClientNotes + "</td></tr>" +
                            "<tr><td>Notes</td><td>" + req.notes + "</td></tr>" +
                            "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                        }
                        else
                        {
                        //og = " " + req.returnDate;
                        string returnDate = " " + req.returnDate?.ToString("dd-MM-yyyy");

                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                            "<tr><th>Field</th><th>Value</th></tr></thead>" +
                            "<tbody  style='line-height: 2;font-size: 12px;'>" +
                            "<tr><td>Service</td><td>" + serv.name + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + part.tradeName + " </td></tr>"
                             + "<tr><td> Agent </td><td> " + agentUser.Partner.tradeName + " </td></tr>"
                             + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                             "<tr><td>Time</td><td>" + req.eventTime + "</td></tr>" +
                             "<tr><td>Client name</td><td>" + req.contactName + "</td></tr>" +
                             "<tr><td>Client e-mail</td><td>" + req.contactEmail + "</td></tr>" +
                             "<tr><td>Client phone</td><td>" + req.contactPhone + "</td></tr>" +
                             "<tr><td>Nº of persons</td><td>" + req.nrPersons + "</td></tr>" +
                             "<tr><td>Price</td><td>" + req.price + "</td></tr>" +
                             "<tr><td>Pick up location</td><td>" + req.pickupLocation + "</td></tr>" +
                             "<tr><td>Dropoff location</td><td>" + req.dropoffLocation + "</td></tr>" +
                             "<tr><td>Flight number</td><td>" + req.flightNr + "</td></tr>" +
                             "<tr><td>Return Date</td><td>" + returnDate + "</td></tr>" +
                             "<tr><td>Return Time</td><td>" + req.returnTime + "</td></tr>" +
                             "<tr><td>Return flight number</td><td>" + req.returnFlight + "</td></tr>" +
                             "<tr><td>Return pickup</td><td>" + req.returnPickup + "</td></tr>" +
                             "<tr><td>Return dropoff</td><td>" + req.returnDropoff + "</td></tr>" +
                             "<tr><td>Client notes</td><td>" + req.ClientNotes + "</td></tr>" +
                             "<tr><td>Notes</td><td>" + req.notes + "</td></tr>" +
                             "<tr><td>Operator notes</td><td>" + req.OperatorNotes + "</td></tr></tbody></table>";
                        }


                        string time = System.DateTime.Now.ToString("HH:mm");
                        string content = email.AgentContent(user.fullName,time) + Changes + reqDetails;

                        email.Email_to_send(agentUser.Partner.email, url, content, subject);

                        url = "ServicesRequested/Details/" + req.ID;
                        url = link + url;

                        subject = "Request " + req.Agreement.label + "#" + req.ID + "-" + agentUser.Partner.tradeName + " was changed.";


                        content = email.OperatorEditorContent(part.tradeName, time) + Changes + reqDetails;
                        email.Email_to_send(part.email, url, content, subject);

                        //content = email.OperatorEditorContent() + Changes;
                        //email.Email_to_send(user.email, url, content, subject);

                    }
                }
            catch (DbEntityValidationException dbEx)
            {
                var url = "ServicesRequested/Details";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return RedirectToAction("Index", "Error");
            }
            return RedirectToAction("Index", "ServicesRequested");
        }
    }
}