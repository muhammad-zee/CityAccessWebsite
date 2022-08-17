using CityAccess.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.CalendarComponents;
using System.IO;
using System.Text;

namespace CityAccess
{
    public class PartnersController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Partners
        public ActionResult Index()
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(db.Partners.ToList());
        }

        // GET: Partners/Details/5
        public ActionResult Details(int? id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Partner partner = db.Partners.Find(id);
            if (partner == null)
            {
                return HttpNotFound();
            }
            return View(partner);
        }

        // GET: Partners/Create
        public ActionResult Create(int? inv, int? part)
        {
            if (Session["admin"] == null && Session["userID"] == null && inv != 1)
            {
                return RedirectToAction("Index", "Login");
            }
            if (part != null)
            {
                ViewBag.invitedby = part;
            }
            Partner partner = new Partner();
            return View(partner);
        }

        // POST: Partners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,invitedby,tradeName,email,description,fiscalID,countryID,invoiceName,invoiceAddress,isAgent,isOperator,isActive, file, ContactPerson, ContactPhone, ContactEmail, isPublic")] Partner partner)
        {

            if (Session["admin"] == null && Session["userID"] == null && partner.invitedby == null)
            {
                return RedirectToAction("Index", "Login");
            }



            if (partner.file != null)
            {
                PartnerLogo logo = new PartnerLogo();

                WebImage imgResized = new WebImage(partner.file.InputStream);

                float ratio = (float)imgResized.Width / (float)imgResized.Height;

                if (ratio > 0.7)
                {
                    float w = 50 * ratio;

                    imgResized = imgResized.Resize((int)w, 50);
                    int sizeToCrop = (imgResized.Width - 35) / 2;
                    imgResized = imgResized.Crop(0, sizeToCrop, 0, sizeToCrop);
                }
                else
                {

                    float h = 35 / ratio;
                    imgResized = imgResized.Resize(35, (int)h);
                    int sizeToCrop = (imgResized.Height - 50) / 2;
                    imgResized = imgResized.Crop(sizeToCrop, 0, sizeToCrop, 0);
                }


                    logo.Image = imgResized.GetBytes();

                    logo.PartnerID = partner.ID;
                    db.PartnerLogoes.Add(logo);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    partner.IcalLink = Guid.NewGuid();
                    db.Partners.Add(partner);
                    db.SaveChanges();
                    if(partner.invitedby != null)
                    {
                        return RedirectToAction("PartnerSuccess", new { partID = partner.ID});
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Partners/Create";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return RedirectToAction("Index", "Error");
            }

            return View(partner);
        }

        public ActionResult PartnerSuccess(int? partID)
        {
            Partner part = db.Partners.Find(partID);
            string mail = part.email; 
            EmailController email = new EmailController();

            string subject = "Your City Access partner account creation was successfull.";
            string content = "<br/><br> After your successfull partner registration follow the link below to register your admin user.";

            var verifyUrl = "/Register/Index?inv="+partID;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            email.Email_to_send(mail, link, content, subject);


            return View();
        }

        // GET: Partners/Edit/5
        public ActionResult Edit(int? id, int? origin)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(id);
            //Boolean hasAccess = true;

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

            if(origin != null)
            {
                ViewBag.FromUser = true;
            }

            Partner partner = db.Partners.Find(id);
            if (partner == null)
            {
                return HttpNotFound();
            }

            return View(partner);            
        }

        // POST: Partners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,tradeName,email,description,fiscalID,countryID,invoiceName,invoiceAddress,isAgent,isOperator,isActive, file, ContactPerson, ContactPhone, ContactEmail, isPublic, FromUser")] Partner partner)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (partner.file != null)
            {
                PartnerLogo logo = db.PartnerLogoes.Where(z => z.PartnerID == partner.ID).FirstOrDefault();

                int control = 0;


                WebImage imgResized = new WebImage(partner.file.InputStream);

                float ratio = (float)imgResized.Width / (float)imgResized.Height;

                if (ratio > 0.7)
                {
                    float w = 50 * ratio;

                    imgResized = imgResized.Resize((int)w, 50);
                    int sizeToCrop = (imgResized.Width - 35) / 2;
                    imgResized = imgResized.Crop(0, sizeToCrop, 0, sizeToCrop);
                }
                else
                {

                    float h = 35 / ratio;
                    imgResized = imgResized.Resize(35, (int)h);
                    int sizeToCrop = (imgResized.Height - 50) / 2;
                    imgResized = imgResized.Crop(sizeToCrop, 0, sizeToCrop, 0);
                }


                if (logo == null)
                {
                    control = 1;
                    logo = new PartnerLogo();
                    logo.PartnerID = partner.ID;
                }
                logo.Image = imgResized.GetBytes();
                if (control == 0)
                {
                    db.Entry(logo).State = EntityState.Modified;
                }
                else
                {
                    db.PartnerLogoes.Add(logo);
                }

            }

            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(partner).State = EntityState.Modified;
                    db.SaveChanges();
                    if (partner.FromUser != true)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Agreements");
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Partners/Edit";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return RedirectToAction("Index", "Error");
            }

            return View(partner);
        }


        //Creates the ical file with SB and flushes it
        public ActionResult Ical(Guid s)
        {
            //var requests = db.Requests.Where((x => Encryptor.MD5Hash(x.Agreement.partnerID.ToString()) == s || Encryptor.MD5Hash(x.Agreement.Service.operatorID.ToString()) == s) && ( => b.stateID== "Submitted" || x.stateID == "Approved"));

            var partner = db.Partners.Where(a => a.IcalLink == s).FirstOrDefault();
            var partnerID = partner.ID;


            DateTime LimitDate = DateTime.Today.AddMonths(-1); 

            var requests1 = from req in db.Requests
                           join ag in db.Agreements on req.agreementID equals ag.ID
                           join serv in db.Services on ag.serviceID equals serv.ID
                           where (req.Agreement.partnerID == partnerID || req.Agreement.Service.operatorID == partnerID) 
                            && (req.stateID == "Submitted" || req.stateID == "Approved") 
                            && (req.eventDate >= LimitDate)
                            select new Req_forTransfer
                            {
                                ID = req.ID,
                                AgreementID = req.agreementID,
                                BookerId = req.bookerId,
                                ContactName = req.contactName,
                                ContactEmail = req.contactEmail,
                                ContactPhone = req.contactPhone,
                                EventDate = req.eventDate,
                                EventTime = req.eventTime,
                                NrPersons = req.nrPersons,
                                Price = req.price,
                                Reference = req.reference,
                                Notes = req.notes,
                                PickupLocation = req.pickupLocation,
                                DropoffLocation = req.dropoffLocation,
                                FlightNr = req.flightNr,
                                StateID = req.stateID,
                                BookingDate = req.bookDate,
                                OperatorNotes = req.OperatorNotes,
                                AgOp = req.Agreement.Partner.tradeName,
                                Aglabel = req.Agreement.label,
                                Servlabel = serv.name,
                                operatorID = req.Agreement.Service.operatorID,
                                logo = req.Agreement.Partner.PartnerLogoes.FirstOrDefault().Image,
                                Duration = serv.Duration,
                                eventID = req.eventID

                            };

            var requests2 = from req in db.Requests
                            join ag in db.Agreements on req.agreementID equals ag.ID
                            join serv in db.Services on ag.serviceID equals serv.ID
                            where (req.Agreement.partnerID == partnerID || req.Agreement.Service.operatorID == partnerID)
                             && (req.stateID == "Submitted" || req.stateID == "Approved")
                             && (req.returnDate != null && req.returnDate >= LimitDate)
                            select new Req_forTransfer
                            {
                                ID = req.ID,
                                AgreementID = req.agreementID,
                                BookerId = req.bookerId,
                                ContactName = req.contactName,
                                ContactEmail = req.contactEmail,
                                ContactPhone = req.contactPhone,
                                EventDate = req.returnDate,
                                EventTime = req.returnTime,
                                NrPersons = req.nrPersons,
                                Price = new decimal(),
                                Reference = req.reference,
                                Notes = req.notes,
                                PickupLocation = req.returnPickup,
                                DropoffLocation = req.returnDropoff,
                                FlightNr = req.returnFlight,
                                StateID = req.stateID,
                                BookingDate = req.bookDate,
                                OperatorNotes = req.OperatorNotes,
                                AgOp = req.Agreement.Partner.tradeName,
                                Aglabel = req.Agreement.label,
                                Servlabel = serv.name,
                                operatorID = req.Agreement.Service.operatorID,
                                logo = req.Agreement.Partner.PartnerLogoes.FirstOrDefault().Image,
                                Duration = serv.Duration,
                                eventID = req.eventID
    

                            };

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

                        if (Req_List[i].PartnerID != null)
                        {
                            parts[i] = (int)Req_List[i].PartnerID;
                        }
                        else
                        {
                            parts[i] = 0;
                        }

                        partners = Req_List[i].AgOp;
                        clientName = Req_List[i].ContactName;
                        nrPersons = (int)Req_List[i].NrPersons;
                        status = Req_List[i].EventStatus;
                        notes = Req_List[i].EventNotes;
                    }
                    else
                    {
                        int IDatual = 0;

                        if (Req_List[i].PartnerID != null)
                        {
                            IDatual = (int)Req_List[i].PartnerID;
                        }

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

                        if (Req_List[i].PartnerID != null)
                        {
                            parts[i] = (int)Req_List[i].PartnerID;
                        }
                        else
                        {
                            parts[i] = 0;
                        }

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

            var calendar = new Ical.Net.Calendar();

            string title = string.Empty;

            DateTime eventDate = new DateTime();

            TimeSpan EvDuration;

            string description = string.Empty;

            foreach (var req in Req_List)
            {
                if(req.Aglabel != null)
                {
                    title = req.Aglabel;
                }
                else
                {
                    title = req.Servlabel;
                }

                if (req.eventID != null)
                {
                    title = title + "-PAX: " + req.NrPersons;
                }
                else
                {
                    title = title + "-" + req.ContactName + "-PAX: " + req.NrPersons;
                }

                eventDate = new DateTime(req.EventDate.Value.Year, req.EventDate.Value.Month, req.EventDate.Value.Day, req.EventTime.Value.Hours, req.EventTime.Value.Minutes, req.EventTime.Value.Seconds);

                if(req.Duration == null) {
                    EvDuration = new TimeSpan(0,30,0);
                    }
                else
                {
                    EvDuration = (TimeSpan)req.Duration;
                }

                description = req.NrPersons + " PAX-" + req.ContactName;


                description = "Partner: " + req.AgOp + Environment.NewLine +
                    "Date: " + req.EventDate.Value.ToString("dd/MM/yyyy") + Environment.NewLine +
                    "Time: " + req.EventTime + Environment.NewLine +
                    "Client name: " + req.ContactName + Environment.NewLine;

                if(req.eventID == null)
                {
                    description = description + "Phone number: " +req.ContactPhone + Environment.NewLine;
                }

                description = description +
                    "Number of persons: " + req.NrPersons + Environment.NewLine +
                    "Status: " + req.StateID + Environment.NewLine +
                    "Notes: " + req.Notes;

                calendar.Events.Add(new CalendarEvent
                {
                    Class = "PUBLIC",
                    Summary = title,
                    Created = new CalDateTime(DateTime.Now),
                    Description = description,
                    Start = new CalDateTime(eventDate.ToUniversalTime()),
                    End = new CalDateTime(eventDate.ToUniversalTime().Add(EvDuration)),
                    Sequence = 0,
                    Uid = Guid.NewGuid().ToString()

                });
            }

            var serializer = new CalendarSerializer(new SerializationContext());
            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);

            return File(bytesCalendar, "text/calendar", "event.ics");
        }



        // GET: Partners/Delete/5
        public ActionResult Delete(int? id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Partner partner = db.Partners.Find(id);
            if (partner == null)
            {
                return HttpNotFound();
            }
            return View(partner);
        }

        // POST: Partners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            Partner partner = db.Partners.Find(id);
            PartnerLogo partLog = db.PartnerLogoes.Where(x => x.PartnerID == id).FirstOrDefault();

            try
            {
                if (partLog != null)
                {
                    db.PartnerLogoes.Remove(partLog);
                }
                db.Partners.Remove(partner);
                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Partners/DeleteConfirmed";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return RedirectToAction("Index", "Error");
            }

            return RedirectToAction("Index");
        }


        // GET: Partners/Delete/5
        public ActionResult Invite()
        {
            return View();
        }

        // POST: Partners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Invite([Bind(Include = "email,description")] Partner partner)
        {
            //Session["partner"] = partner.tradeName;
            //Session["partnerID"] = userInfo.partnerId;


            int partnerID = (int)Session["partnerID"];
            string tradename = (string)Session["partner"];

            EmailController email = new EmailController();

            string subject = tradename +" invited you to join City Access";
            string content = partner.description + "<br/><br> Follow the link below to create your partner account.";

            var verifyUrl = "/Partners/Create?inv=1&part="+partnerID;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            email.Email_to_send(partner.email, link, content, subject);

            ViewBag.EmailSent = 1;

            return View();
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
