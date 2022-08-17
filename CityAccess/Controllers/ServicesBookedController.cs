using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Net.Mail;
using System.Reflection;
using System.IO;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text;
using System.Diagnostics;
//using PdfSharp;
//using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Drawing;
using iText;
using iText.Html2pdf;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Layout.Borders;


namespace CityAccess.Controllers
{
    public class ServicesBookedController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();



        // Function that returns a Json with query results according to the filters received on input
        // Inputs: Status - request status, Operator - request operator, ServTitle - service title
        //
        //Originally created to fill calendar view
        public JsonResult Active(string Status, string Operator, string ServTitle)
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
                            AgOp = requ.Agreement.Service.Partner.tradeName,
                            Aglabel = requ.Agreement.label,
                            Servlabel = serv.name,
                            operatorID = requ.Agreement.Partner.ID,
                            logo = requ.Agreement.Service.Partner.PartnerLogoes.FirstOrDefault().Image,
                            Duration = serv.Duration,
                            eventID = requ.eventID,
                            EventNotes = ev.notes,
                            EventStatus = ev.stateID

                        };

            if (Operator != "")
            {
                requests1 = requests1.Where(a => a.AgOp == Operator);
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
            }
            requests1 = requests1.Where(a => a.operatorID == partnerID);// has to change
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
                           AgOp = requ.Agreement.Service.Partner.tradeName,
                           Aglabel = requ.Agreement.label,
                           Servlabel = serv.name,
                           operatorID = requ.Agreement.Partner.ID,
                           logo = requ.Agreement.Service.Partner.PartnerLogoes.FirstOrDefault().Image,
                           Duration = serv.Duration,
                           eventID = requ.eventID,
                           EventNotes = ev.notes,
                           EventStatus = ev.stateID
                       };

            requests2 = requests2.Where(a => a.EventDate != null);

            if (Operator != "")
            {
                requests2 = requests2.Where(a => a.AgOp == Operator);
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
            }
            requests2 = requests2.Where(a => a.operatorID == partnerID); //change to adapt to servicesBooked
            requests2 = requests2.OrderBy(a => a.EventDate + " " + a.EventTime);

            var requests = requests1.Union(requests2).OrderBy(x => x.eventID);

            List<CityAccess.Req_forTransfer> Req_List = requests.ToList();

            int eventID = 0;
            int index =  0;
            string partners = string.Empty;
            string clientName = string.Empty;
            int nrPersons = 0;
            string status = string.Empty;
            string notes = string.Empty;

            int limit = Req_List.Count;

            for (int i=0; i < Req_List.Count; i++)
            {
                if(Req_List[i].eventID != null)
                {
                    if(Req_List[i].eventID != eventID)
                    {
                        if(eventID != 0)
                        {
                            Req_List[index].ID = eventID;
                            Req_List[index].AgOp = partners;
                            Req_List[index].ContactName = clientName;
                            Req_List[index].NrPersons = nrPersons;
                            Req_List[index].StateID = status;
                            Req_List[index].Notes = notes;
                        }

                        eventID = (int)Req_List[i].eventID;
                        index = i;

                        partners = Req_List[i].AgOp;
                        clientName = Req_List[i].ContactName;
                        nrPersons = (int)Req_List[i].NrPersons;
                        status = Req_List[i].EventStatus;
                        notes = Req_List[i].EventNotes;
                    }
                    else
                    {
                        clientName = clientName + ", " + Req_List[i].ContactName;
                        nrPersons = nrPersons + (int)Req_List[i].NrPersons;

                        Req_List.RemoveAt(i);
                        i--;
                    }
                }
            }

            return new JsonResult { Data = Req_List.OrderBy(x => x.EventDate + " " + x.EventTime), JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };

        }





        // Returns a BookingFilters class to populate the active booked services page of an agent 
        // Inputs: int c - is not null when callendar view is requested, then proceeds to call callendar view
        //
        // GET: ServicesBooked
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
                Partner partner = db.Partners.FirstOrDefault(); //maybe not the best approach
                partnerID = partner.ID;
            }


                var requests1 =
                                from requ in db.Requests
                                join us in db.Users on requ.bookerId equals us.ID
                                join ag in db.Agreements on requ.agreementID equals ag.ID
                                join part in db.Partners on ag.Service.operatorID equals part.ID
                                join ev in db.Events on requ.eventID equals ev.ID
                                into evnt
                                from ev in evnt.DefaultIfEmpty()
                                where ag.partnerID == partnerID && (requ.stateID == "Submitted" || requ.stateID == "Approved") && (requ.eventDate >= System.DateTime.Today)
                                orderby requ.eventDate, requ.eventTime
                                select new Req_User
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
                                          EventNotes = ev.notes
                                      },
                                      Partner = part,
                                      Agreement = ag,
                                      User = us
                                  };

            var requests2 =
                  from requ in db.Requests
                  join us in db.Users on requ.bookerId equals us.ID
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join part in db.Partners on ag.Service.operatorID equals part.ID
                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()
                  where ag.partnerID == partnerID && (requ.stateID == "Submitted" || requ.stateID == "Approved") && (requ.returnDate != null && requ.returnDate >= System.DateTime.Today)
                  orderby requ.eventDate, requ.eventTime
                  select new Req_User
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
                          EventNotes = ev.notes
                      },
                      Partner = part,
                      Agreement = ag,
                      User = us
                  };         

            ViewBag.stateID = db.States;
            ViewBag.partners = db.Partners;

            ViewBag.operators = (from ag in db.Agreements
                            join serv in db.Services on ag.serviceID equals serv.ID
                            join part in db.Partners on serv.operatorID equals part.ID
                            where ag.partnerID == partnerID
                            select part).Distinct();



            Partner partn = db.Partners.Find(partnerID);


            if (partn.isAgent == true && partn.isOperator != true)
            {
                ViewBag.Service = (from serv in db.Services
                                   join ag in db.Agreements on serv.ID equals ag.serviceID
                                   where ag.partnerID == partnerID || ag.partnerID == null
                                   select serv).Distinct();
            }
            else
            {
                if (partn.isOperator == true && partn.isAgent != true)
                {
                    ViewBag.Service = (from serv in db.Services
                                       join ag in db.Agreements on serv.ID equals ag.serviceID
                                       where serv.operatorID == partnerID
                                       select serv).Distinct();
                }
                else
                {
                    if(partn.isOperator == true && partn.isAgent == true)
                    {
                        ViewBag.Service = (from serv in db.Services
                                           join ag in db.Agreements on serv.ID equals ag.serviceID
                                           where serv.operatorID == partnerID || ag.partnerID == partnerID || ag.partnerID == null
                                           select serv).Distinct();

                    }
                }
            }
            var requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.eventID).OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);

            List<Req_User> reqs = requests.ToList();
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


            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
            items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
            items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
            items.Add(new SelectListItem() { Text = "Site Approval", Value = "Site Approval" });
            items.Add(new SelectListItem() { Text = "Site Canceled", Value = "Site Canceled" });
            items.Add(new SelectListItem() { Text = "All", Value = "All" });

            BookingFilters bookingFilters = new BookingFilters { Req_UserList = reqs, StatusFilters = items, Status = string.Empty };

            if (c == 1)
            {
                return View("~/Views/ServicesBooked/Calendar.cshtml", bookingFilters);
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
        // Post: ServicesBooked
        [HttpPost]
        public ActionResult Index([Bind(Include = "Status, Partner, Req_UserList, Dwn, Operator, ServTitle, Date, BookingDate, Date2, BookingDate2")]  BookingFilters bookingFilters)
        {

            IQueryable<Req_User> queryable = null;
            var requests = queryable;

            Partner partn;
            int partnerID;

             if(Session["admin"] == null)
            {
                //needed for the database fetch
                partnerID = (int)Session["partnerID"];

            }
            else
            {
                partn = db.Partners.Where(a => a.tradeName == bookingFilters.Partner).FirstOrDefault();
                partnerID = partn.ID;
            }


            var requests1 =
                        from requ in db.Requests
                        join us in db.Users on requ.bookerId equals us.ID
                        join ag in db.Agreements on requ.agreementID equals ag.ID
                        join part in db.Partners on ag.Service.operatorID equals part.ID
                        join ev in db.Events on requ.eventID equals ev.ID
                        into evnt
                        from ev in evnt.DefaultIfEmpty()
                        select new Req_User
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
                                EventNotes = ev.notes
                            },
                            Partner = part,
                            Agreement = ag,
                            User = us
                        };

            //filtering part
            if (bookingFilters.Operator != null)
            {
                requests1 = requests1.Where(a => a.Partner.tradeName == bookingFilters.Operator);
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
            }
            requests1 = requests1.Where(a => a.Agreement.partnerID == partnerID);
            requests1 = requests1.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);


            var requests2 =
                  from requ in db.Requests
                  join us in db.Users on requ.bookerId equals us.ID
                  join ag in db.Agreements on requ.agreementID equals ag.ID
                  join part in db.Partners on ag.Service.operatorID equals part.ID
                  join ev in db.Events on requ.eventID equals ev.ID
                  into evnt
                  from ev in evnt.DefaultIfEmpty()
                  select new Req_User
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
                          EventNotes = ev.notes
                      },
                      Partner = part,
                      Agreement = ag,
                      User = us
                  };


            //filtering part
            requests2 = requests2.Where(a => a.req_ForTransfer.EventDate != null);

            if (bookingFilters.Operator != null)
            {
                requests2 = requests2.Where(a => a.Partner.tradeName == bookingFilters.Operator);
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
            }
            requests2 = requests2.Where(a => a.Agreement.partnerID == partnerID);
            requests2 = requests2.OrderBy(a => a.req_ForTransfer.EventDate + " " + a.req_ForTransfer.EventTime);





            requests = requests1.Union(requests2).OrderBy(x => x.req_ForTransfer.EventDate + " "+ x.req_ForTransfer.EventTime);



            List<Req_User> reqs = requests.ToList();
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


            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem() { Text = "Submitted", Value = "Submitted" });
            items.Add(new SelectListItem() { Text = "Approved", Value = "Approved" });
            items.Add(new SelectListItem() { Text = "Canceled", Value = "Canceled" });
            items.Add(new SelectListItem() { Text = "Invoiced", Value = "Invoiced" });
            items.Add(new SelectListItem() { Text = "Site Approval", Value = "Site Approval" });
            items.Add(new SelectListItem() { Text = "Site Canceled", Value = "Site Canceled" });
            items.Add(new SelectListItem() { Text = "All", Value = "All" });



                bookingFilters.StatusFilters = items;
                bookingFilters.Req_UserList = reqs;
                ViewBag.stateID = db.States;
                ViewBag.partners = db.Partners;

                //Excel part
                if (bookingFilters.Dwn == 1)
                {
                    StringWriter sw = new StringWriter();
                    var requestsToUs = new System.Data.DataTable("requests");

                    requestsToUs.Columns.Add("Request", typeof(string));
                    requestsToUs.Columns.Add("Operator", typeof(string));
                    requestsToUs.Columns.Add("Partner", typeof(string));
                    requestsToUs.Columns.Add("Service Name", typeof(string));
                    requestsToUs.Columns.Add("Booker Name", typeof(string));
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

                foreach (var item in bookingFilters.Req_UserList)
                    {

                    if (item.Partner == null)
                    {
                        partnerTradename = "General Request";
                    }
                    else
                    {
                        partnerTradename = item.Partner.tradeName;
                    }

                    requestsToUs.Rows.Add(
                            item.req_ForTransfer.ID,
                            partnerTradename,
                            item.User.Partner.tradeName,
                            item.Agreement.label,
                            item.User.fullName,
                            item.req_ForTransfer.ContactName,
                            item.req_ForTransfer.EventDate.Value.ToString("dd/MM/yyyy"), //Short date string removes the hours from the date
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
                    }

                    var grid = new GridView();
                    grid.DataSource = requestsToUs;
                    grid.DataBind();

                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachement;filename=ServicesBooked.xls");
                    Response.ContentType = "application/ms-excel";

                    HtmlTextWriter htw = new HtmlTextWriter(sw);

                    grid.RenderControl(htw);

                    Response.Write(sw.ToString());
                    Response.Flush();
                    Response.End();
                }

                ViewBag.operators = (from ag in db.Agreements
                                     join serv in db.Services on ag.serviceID equals serv.ID
                                     join part in db.Partners on serv.operatorID equals part.ID
                                     where ag.partnerID == partnerID
                                     select part).Distinct();

            Partner partnA = db.Partners.Find(partnerID);


            if (partnA.isAgent == true && partnA.isOperator != true)
            {
                ViewBag.Service = (from serv in db.Services
                                   join ag in db.Agreements on serv.ID equals ag.serviceID
                                   where ag.partnerID == partnerID || ag.partnerID == null
                                   select serv).Distinct();
            }
            else
            {
                if (partnA.isOperator == true && partnA.isAgent != true)
                {
                    ViewBag.Service = (from serv in db.Services
                                       join ag in db.Agreements on serv.ID equals ag.serviceID
                                       where serv.operatorID == partnerID
                                       select serv).Distinct();
                }
                else
                {
                    if (partnA.isOperator == true && partnA.isAgent == true)
                    {
                        ViewBag.Service = (from serv in db.Services
                                           join ag in db.Agreements on serv.ID equals ag.serviceID
                                           where serv.operatorID == partnerID || ag.partnerID == partnerID || ag.partnerID == null
                                           select serv).Distinct();
                    }
                }
            }

            return View(bookingFilters);            
        }


        //Loads request edit view with the given ID
        //Constructs the Html e-mail to send o client 
        //
        //Input: int? id - id of the request to be edited
        // GET: ServicesBooked/Edit/5
        public ActionResult Edit(int? id)
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
            if (request == null)
            {
                return HttpNotFound();
            }

            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(request.Agreement.Partner.ID);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }



            var ag = db.Agreements.Where(z => z.ID == request.agreementID).FirstOrDefault();
            var serv = db.Services.Where(x => x.ID == ag.serviceID).FirstOrDefault();
            var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
            var opr = db.Partners.Where(a => a.ID == serv.operatorID).FirstOrDefault();

            if (servType.hasReturn == true)
            {
                ViewBag.Control = "return";
            }
            if (servType.isTransfer != true)
            {
                ViewBag.NotTransfer = "notTransfer";
            }

            var reqLog_list = db.RequestLogs.Where(z => z.requestID == id);


            //Generate Select list item from database query
            var states = db.StateTransitions.Where(a => a.Origin == request.stateID).ToList().Select(u => new SelectListItem
            {
                Text = u.Destiny,
                Value = u.Destiny
            });


            //Because Ienumerable<List> doesnt allow the addition of items to the list , we create a list with one item and then concat the two lists
            var itemToAdd = new SelectListItem{
                Text = request.stateID,
                Value = request.stateID
            };

            List<SelectListItem> Origin = new List<SelectListItem>();
            Origin.Add(itemToAdd);

            states = states.Concat(Origin);

            ViewBag.stateIDs = states;

            //User responsible = db.Users.Find(request.ResponsibleId)

            ReqLog reqLog = new ReqLog { Request = request, ReqLog_List = reqLog_list.ToList(), stateID = request.stateID, Agreement = ag, ResponsibleID = request.User1?.fullName };

            ///////////////// Set up for the confirmation e-mail ///////////////////////////////////////////////////////

            string og = " " + request.eventDate.ToString("dd-MM-yyyy");

            reqLog.ClientMailSubject = "Booking: " + serv.name + ":" + og + "-" + request.eventTime;

            string content = "Your booking is confirmed with the following details:";

            string confirmationText;
            string cancellationPolicy;
            string servLabel;

            if (request.Agreement.messageTemplate != null)
            {
                confirmationText = request.Agreement.messageTemplate;
            }
            else
            {
                confirmationText = request.Agreement.Service.ConfirmationText;
            }


            if (request.Agreement.cancellationPolicy != null)
            {
                cancellationPolicy = request.Agreement.cancellationPolicy;
            }
            else
            {
                cancellationPolicy = request.Agreement.Service.cancellationPolicy;
            }

            if (request.Agreement.label != null)
            {
               servLabel = request.Agreement.label;
            }
            else
            {
                servLabel = serv.name;
            }



            if (request.returnDate == null)
            {

                content = content + "<br/><br><table contenteditable='true' cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Service</td><td>" + servLabel + "</td></tr>"
                    + "<tr><td style ='background-color:rgb(63,150,170);color:white;'> Date </td><td> " + og + " </td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Time</td><td>" + request.eventTime + "</td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client name</td><td>" + request.contactName + "</td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Price</td><td>" + request.price + "</td></tr>";
                if (request.pickupLocation != null)
                {
                    content = content +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Pick up location</td><td>" + request.pickupLocation + "</td></tr>";
                }
                if (request.dropoffLocation != null)
                {
                    content = content +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>";
                }
                if (request.flightNr != null)
                {
                    content = content +
                    "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Flight number</td><td>" + request.flightNr + "</td></tr>";
                }
                content = content +
                    "</table><br/><br><p style='font-size:12px;'>" + confirmationText +"</p>"+
                    "<p style='font-size:12px;'><b>Cancellation Policy</b></p>" +
                    "<p style='font-size:12px;'>" + cancellationPolicy +"</p>";
            }
            else
            {
                //og = " " + req.returnDate;
                string returnDate = " " + request.returnDate?.ToString("dd-MM-yyyy");

                content = content + "<br/><br><table contenteditable='true' cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'>"
                     + "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Service</td><td>" + servLabel + "</td></tr>"
                     + "<tr><td style ='background-color:rgb(63,150,170);color:white;'> Date </td><td> " + og + " </td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Time</td><td>" + request.eventTime + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client name</td><td>" + request.contactName + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Price</td><td>" + request.price + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Pick up location</td><td>" + request.pickupLocation + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Flight number</td><td>" + request.flightNr + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Return Date</td><td>" + returnDate + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Return Time</td><td>" + request.returnTime + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Return flight number</td><td>" + request.returnFlight + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Return pickup</td><td>" + request.returnPickup + "</td></tr>" +
                     "<tr><td style ='background-color:rgb(63,150,170);color:white;'>Return dropoff</td><td>" + request.returnDropoff + "</td></tr>" +
                     "</table>" +
                    "</table><br/><br><p style='font-size:12px;'>" + confirmationText + "</p>" +
                    "<p style='font-size:12px;'><b>Cancellation Policy</b></p>" +
                    "<p style='font-size:12px;'>" + cancellationPolicy + "</p>";
            }

            reqLog.ClientMailContent = BreakLN.ProcessBrkHTML(content);



            return View(reqLog);
        }


        //This function has 3 purposes
        //1.Saving edited data about the request to the DB
        //2.Present a printable voucher to the agent with request details
        //3.Send a confirmation e-mail to the client - only when request is approved or invoiced
        //
        // POST:  ServicesBooked/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ID, bookerId, agreementID, flightNr,contactName, notes, contactPhone, eventDate, eventTime, nrPersons, pickupLocation, price, reference, dropoffLocation,returnFlight,returnDate, returnTime, returnPickup, returnDropoff, contactEmail, extraDate1, extraDate2, extraDate3, extraTime1, extraTime2, extraTime3, extraText1, extraText2, extraText3, extraMultiText1, extraMultiText2, extraMultiText3")]  Request request, String stateID, [Bind(Include ="Vouch")]ReqLog reqLog1, String content1, String Subject)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Variables used both in the edit of the booking as well on the voucher
            db.Configuration.ProxyCreationEnabled = false;
            var previous_request = db.Requests.Where(x => x.ID == request.ID).FirstOrDefault();
            var user = db.Users.Find((int)Session["userID"]);
            var part = db.Partners.Find((int)Session["partnerID"]);
            var ag = db.Agreements.Where(z => z.ID == previous_request.agreementID).FirstOrDefault();
            var serv = db.Services.Where(p => p.ID == ag.serviceID).FirstOrDefault();
            var opr = db.Partners.Where(a => a.ID == serv.operatorID).FirstOrDefault();

            //E-mail to client 
            if (content1 != null)
            {
                EmailController email = new EmailController();

                string emailToSend;
                int k = 0;
                string myString = new string(' ', k + 132);
                int j = 0;
                StringBuilder sb;

                if (request.contactEmail != null)
                {
                    k = request.contactEmail.Length;
                    sb = new StringBuilder(myString, 100000);

                    for (int i = 0; i < request.contactEmail.Length; i++)
                    {
                        if (request.contactEmail[i] == ',')
                        {
                            k = 1;
                        }

                    }

                    if (k == 1)
                    {
                        for (int i = 0; i < request.contactEmail.Length; i++)
                        {
                            if (request.contactEmail[i] != ',')
                            {
                                sb[j] = request.contactEmail[i];
                                j++;
                            }
                            else
                            {
                                if (request.contactEmail[i - 1] != ',')
                                {
                                    emailToSend = sb.ToString();
                                    j = 0;
                                    sb = new StringBuilder(myString, 100000);
                                    email.Email_to_send(emailToSend, null, content1, Subject);
                                }
                            }
                        }
                    }
                    else
                    {
                        email.Email_to_send(request.contactEmail, null, content1, Subject);
                    }
                }
                return RedirectToAction("Index", "ServicesBooked");
            }

            //Pdf part to form printable voucher
            if (reqLog1.Vouch == 1)
            { 
                string confirmationText;
                string cancellationPolicy;
                string title;



                object aux = Nullcheck(previous_request);
                previous_request = (Request)aux;

                if (previous_request.Agreement.messageTemplate != null)
                {
                    confirmationText = previous_request.Agreement.messageTemplate;
                }
                else
                {
                    if (previous_request.Agreement.Service.ConfirmationText != null)
                    {
                        confirmationText = previous_request.Agreement.Service.ConfirmationText;
                    }
                    else
                    {
                        confirmationText = " ";
                    }
                }


                if (previous_request.Agreement.cancellationPolicy != null)
                {
                    cancellationPolicy = previous_request.Agreement.cancellationPolicy;
                }
                else
                {
                    if (previous_request.Agreement.Service.cancellationPolicy != null)
                    {
                        cancellationPolicy = previous_request.Agreement.Service.cancellationPolicy;
                    }
                    else
                    {
                        cancellationPolicy = " ";
                    }
                }
                if(ag.label == null)
                {
                    title = serv.name;
                }
                else
                {
                    title = ag.label;
                }


                MemoryStream workstream = new MemoryStream();
                var writer = new PdfWriter(workstream); //write to the given memory stream
                writer.SetCloseStream(false);

                PdfDocument pdf = new PdfDocument(writer);

                PageSize ps = PageSize.A4;
                Document document = new Document(pdf, ps);
                var page = pdf.AddNewPage(ps);

                float docWidth = ps.GetWidth();
                float docHeight = ps.GetHeight();

                document.SetMargins(20, 20, 20, 20);

                //Adding the logos to the page top
                PartnerLogo partLogo = db.PartnerLogoes.Where(x => x.PartnerID == opr.ID).FirstOrDefault();
                PartnerLogo agLogo = db.PartnerLogoes.Where(x => x.PartnerID == ag.Partner.ID).FirstOrDefault();

                ImageData oprLogo = ImageDataFactory.Create(partLogo.Image);
                iText.Layout.Element.Image OperatorLogo = new iText.Layout.Element.Image(oprLogo).ScaleAbsolute(38, 53).SetFixedPosition(35, 750);
                document.Add(OperatorLogo);

                if (agLogo != null)
                {
                    ImageData agentLogo = ImageDataFactory.Create(agLogo.Image);
                    iText.Layout.Element.Image AgentImg = new iText.Layout.Element.Image(agentLogo).ScaleAbsolute(38, 53).SetFixedPosition(515, 750);
                    document.Add(AgentImg);
                }
                //////////////////


                var canvas = new PdfCanvas(page);
                canvas.SetLineWidth(1)
                    .SetStrokeColorRgb(0, (float)0.329, (float)0.45).MoveTo(35, 740)
                    .LineTo(553, 740)
                    .Stroke();

                var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                document.Add(new Paragraph("Thanks for your booking !").SetFont(font).SetFixedPosition(docWidth / 2 - 75, 715, 400));


                canvas.SetLineWidth(1)
                    .SetStrokeColorRgb(0, (float)0.329, (float)0.45).MoveTo(35, 710)
                    .LineTo(553, 710)
                    .Stroke();

                //First table
                document.Add(new Paragraph("Client Details").SetFont(font).SetFixedPosition(35, 670, 100));

                iText.Kernel.Colors.Color cellbck = new DeviceRgb(63, 150, 170);
                iText.Kernel.Colors.Color fontcolor = new DeviceRgb(255, 255, 255);


                iText.Layout.Element.Table client = new iText.Layout.Element.Table(new float[] { 5, 7 }).SetFixedPosition(35, 600, 200).SetFontSize((float)9).SetBorder(new SolidBorder(cellbck, 1));

                Cell cell = new Cell().Add(new Paragraph("Name")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.contactName)).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                cell = new Cell().Add(new Paragraph("Email")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.contactEmail)).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                cell = new Cell().Add(new Paragraph("Phone")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.contactPhone)).SetBorder(new SolidBorder(cellbck, 1));
                client.AddCell(cell);

                document.Add(client);
                ////////////////////////////////////////////////////

                //Second table
                document.Add(new Paragraph(title).SetFont(font).SetFixedPosition(280, 670, 200));

                iText.Layout.Element.Table service = new iText.Layout.Element.Table(new float[] { 8, 11 }).SetFontSize((float)9).SetBorder(new SolidBorder(cellbck, 1));
                service.SetFixedLayout();

                cell = new Cell().Add(new Paragraph("Date")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.eventDate.ToString("dd-MM-yyyy"))).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph("Time")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.eventTime.ToString())).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph("Nº of Persons")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.nrPersons.ToString())).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph("Price")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                cell = new Cell().Add(new Paragraph(previous_request.price.ToString())).SetBorder(new SolidBorder(cellbck, 1));
                service.AddCell(cell);

                if (previous_request.pickupLocation != null)
                {
                    cell = new Cell().Add(new Paragraph("Pick up Location")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.pickupLocation)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);
                }
                if (previous_request.dropoffLocation != null)
                {
                    cell = new Cell().Add(new Paragraph("Dropoff location")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.dropoffLocation)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);
                }
                if (previous_request.flightNr != null)
                {
                    cell = new Cell().Add(new Paragraph("Flight Number")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.flightNr)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);
                }

                if (previous_request.returnDate != null)
                {
                    cell = new Cell().Add(new Paragraph("Return Date")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.returnDate?.ToString("dd-MM-yyyy"))).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph("Return Time")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.returnTime.ToString())).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph("Return flight")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.returnFlight)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph("Return Pick up Location")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.returnPickup)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph("Return DropOff Location")).SetBackgroundColor(cellbck).SetFontColor(fontcolor).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    cell = new Cell().Add(new Paragraph(previous_request.returnDropoff)).SetBorder(new SolidBorder(cellbck, 1));
                    service.AddCell(cell);

                    service.SetFixedPosition(280, 435, 280);
                }
                else
                {
                    service.SetFixedPosition(280, 530, 280);
                }


                document.Add(service);
                //////////////////////////////////////////

                //Finding the number of lines for the confirmation text 
                int i = 0;
                int num_lines = 1;
                int j = 0;

                for(i = 0; i < confirmationText.Length; i++)
                {
                    if(confirmationText[i] == '\r' && confirmationText[i + 1] == '\n')
                    {
                        num_lines ++;
                        j = i;
                    }

                    if(i == (j + 106))
                    {
                        num_lines++;
                        j = i;
                    }
                }

                float line_height = 11;
                float paragraph_bottom = 400 - (line_height * num_lines);
                float rect_bottom = 398 - (line_height * num_lines);
                float rect_height = line_height * num_lines;

                /// Confirmation 
                document.SetFontSize(9);
                canvas.Rectangle(35, rect_bottom, 525, rect_height).SetStrokeColorRgb(0, (float)0.329, (float)0.45).Stroke();
                document.Add(new Paragraph(confirmationText).SetFont(font).SetFixedPosition(37, paragraph_bottom, 523).SetFixedLeading(10));


                //Cancellation Policy
                //Finding the number of lines for the confirmation text 
                 i = 0;
                 num_lines = 1;
                 j = 0;

                for (i = 0; i < cancellationPolicy.Length; i++)
                {
                    if (cancellationPolicy[i] == '\r' && cancellationPolicy[i + 1] == '\n')
                    {
                        num_lines++;
                        j = i;
                    }

                    if (i == (j + 106))
                    {
                        num_lines++;
                        j = i;
                    }
                }

                paragraph_bottom = paragraph_bottom - (line_height * num_lines) - 50;
                rect_bottom = rect_bottom - (line_height * num_lines) - 50;
                rect_height = line_height * num_lines; 

                document.Add(new Paragraph("Cancellation Policy").SetFont(font).SetFixedPosition(35, rect_bottom+rect_height+2, 200));
                canvas.Rectangle(35,rect_bottom, 525, rect_height).SetStrokeColorRgb(0, (float)0.329, (float)0.45).Stroke();
                document.Add(new Paragraph(cancellationPolicy).SetFont(font).SetFixedPosition(37, paragraph_bottom, 450).SetFixedLeading(10));

                //////////////////////////////////////////////////

                //Footer with CA logo
                string imageFile = System.IO.Path.Combine(Server.MapPath("~/Content"), "Logo.png");
                byte[] buffer = Encoding.ASCII.GetBytes(imageFile);

                ImageData CALogo = ImageDataFactory.Create(System.IO.Path.Combine(Server.MapPath("~/Content"), "Logo.png"));
                iText.Layout.Element.Image CA_Logo = new iText.Layout.Element.Image(CALogo).ScaleAbsolute(25, 25).SetFixedPosition(450, 15);
                document.Add(CA_Logo);
                document.Add(new Paragraph("Powered by CityAccess").SetFont(font).SetFixedPosition(480, 20, 115));


                /////////////////////////////////////////////////

                document.Close();

                workstream.Position = 0;
                return new FileStreamResult(workstream, "application/pdf");
            }



            //Manual load of the entities attached to a request -> In the near future do this in a more efficient way
            request.RequestLogs = previous_request.RequestLogs;
            request.OperatorNotes = previous_request.OperatorNotes;
            request.User = previous_request.User;
            request.Agreement = previous_request.Agreement;
            request.State = previous_request.State;
            request.bookDate = previous_request.bookDate;
            request.bookTime = previous_request.bookTime;

            //Because the binding for the dropdown only works if its a direct connection, a stateID was added to the higher abstraction View Model
            request.stateID = stateID;

            Logobj Changes = Log.Changes(previous_request, request);

            var interSiteStates = false;

            if ((previous_request.stateID == "Site Approval" || previous_request.stateID == "Site Canceled") && (request.stateID == "Site Approval" || request.stateID == "Site Canceled")){
                interSiteStates = true;
            }


            // Call for update helper because framework conflicts if model comes directly from the view
            object req = UpdateHelper(previous_request, request);
            req = (Request)req;

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(req).State = EntityState.Modified;

                    RequestLog reqLog = new RequestLog();
                    reqLog.Date = System.DateTime.Now ;
                    reqLog.Time = System.DateTime.Now.ToString("HH:mm");
                    reqLog.requestID = previous_request.ID;
                    reqLog.userID = (int)Session["userID"];
                    reqLog.notes = Changes.changes;
                    db.RequestLogs.Add(reqLog);
                    db.SaveChanges();



                    //Set up and forwarding e-mails

                    EmailController email = new EmailController();
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                    string url = "ServicesBooked/Details/" + previous_request.ID;
                    url = link + url;
                    string subject = "Booking " + ag.label + "#" + request.ID +"-" + opr.tradeName + " was changed." ;

                    //general info about the request
                    string reqDetails = String.Empty;

                    string og = " " + request.eventDate.ToString("dd-MM-yyyy");
                    string date = og.Replace("12:00:00 AM", " ");

                    reqDetails = "<br/><br><p></p>Request general details:<br/><br> ";

                    if (request.returnDate == null)
                    {
                    reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                        + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name + "</td></tr>"
                        + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                        + "<tr><td> Agent </td><td> " + part.tradeName + " </td></tr>"
                        + "<tr><td> Date </td><td> " + og + " </td></tr>" +
                        "<tr><td>Time</td><td>" + request.eventTime + "</td></tr>" +
                        "<tr><td>Client name</td><td>" + request.contactName + "</td></tr>" +
                        "<tr><td>Client e-mail</td><td>" + request.contactEmail + "</td></tr>" +
                        "<tr><td>Client phone</td><td>" + request.contactPhone + "</td></tr>" +
                        "<tr><td>Nº of persons</td><td>" + request.nrPersons + "</td></tr>" +
                        "<tr><td>Price</td><td>" + request.price + "</td></tr>";
                    if (request.pickupLocation != null)
                    {
                        reqDetails = reqDetails +
                        "<tr><td>Pick up location</td><td>" + request.pickupLocation + "</td></tr>";
                    }
                    if (request.dropoffLocation != null)
                    {
                        reqDetails = reqDetails +
                        "<tr><td>Dropoff location</td><td>" + request.dropoffLocation + "</td></tr>";
                    }
                    if (request.flightNr != null)
                    {
                        reqDetails = reqDetails +
                        "<tr><td>Flight number</td><td>" + request.flightNr + "</td></tr>";
                    }
                    reqDetails = reqDetails +
                        "<tr><td>Client notes</td><td>" + request.ClientNotes + "</td></tr>" +
                        "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                        "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                    }
                    else
                    {
                        //og = " " + request.returnDate?.ToString("dd-MM-yyyy");
                        string returnDate = " " + request.returnDate?.ToString("dd-MM-yyyy");

                        reqDetails = reqDetails + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                            + "<tr><th>Field</th><th>Value</th></tr></thead><tbody><tr><td>Service</td><td>" + serv.name + "</td></tr>"
                            + "<tr><td> Operator </td><td> " + opr.tradeName + " </td></tr>"
                            + "<tr><td> Agent </td><td> " + part.tradeName + " </td></tr>"
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
                             "<tr><td>Client notes</td><td>" + request.ClientNotes + "</td></tr>" +
                             "<tr><td>Notes</td><td>" + request.notes + "</td></tr>" +
                             "<tr><td>Operator notes</td><td>" + request.OperatorNotes + "</td></tr></tbody></table>";
                    }



                    //String content = email.EditorContent() + new_chang + reqDetails;

                    //email.Email_to_send(user.email, url, content, subject); // Não é suposto o editor receber mail pessoal
                    string time = System.DateTime.Now.ToString("HH:mm");
                    String content = email.AgentContent(user.fullName, time) + Changes.Htmlchanges + reqDetails;
                    email.Email_to_send(part.email, url, content, subject);

                    url = "ServicesRequested/Edit/" + previous_request.ID;
                    url = link + url;

                    subject = "Request " + ag.label + "#" + request.ID + "-" + part.tradeName + " was changed.";


                    if (!interSiteStates) {
                        content = email.OperatorContent(part.tradeName, time) + Changes.Htmlchanges + reqDetails;
                        email.Email_to_send(opr.email, url, content, subject);
                    }
                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "ServicesBooked/Edit";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);

                    return RedirectToAction("Index", "Error");
                }


                return RedirectToAction("Index");
            }
            return View(request);
        }





        //Function that replaces null fields of type string by "-"
        //Originally created because itext7 voucher doesnt handle null fields
        //
        [NonAction]
        public object Nullcheck(object req)
        {
            string aux = string.Empty;
            System.Diagnostics.Debug.WriteLine(aux.GetType());

            foreach (PropertyInfo propertyInfo in req.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    if (propertyInfo.PropertyType == aux.GetType())
                    {
                        System.Diagnostics.Debug.WriteLine(propertyInfo.GetType());
                        object view_reqValue = propertyInfo.GetValue(req, null);
                        if (view_reqValue == null)
                        {
                            propertyInfo.SetValue(req, "-");
                        }
                    }
                }
            }
            return req;
        }

        //The update of a DB object with new fields return from the view
        //Origanally created because framework was conflicting with model directly from view
        //
        [NonAction]
        public object UpdateHelper(object base_req, object view_req )
        {
            if (base_req.GetType() != view_req.GetType())
            {
                return null;
            }

            foreach(PropertyInfo propertyInfo in base_req.GetType().GetProperties())
            {
                if(propertyInfo.CanRead)
                {
                    object view_reqValue = propertyInfo.GetValue(view_req, null);
                    if(view_reqValue != null)
                    {
                        propertyInfo.SetValue(base_req, view_reqValue);
                    }
                }
            }
            return base_req;
        }
    }
}