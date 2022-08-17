using CityAccess.Controllers;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace CityAccess
{
    public class UsersController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Users
        public ActionResult Index()
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            IQueryable<User> queryable = null;
            var users = queryable;

            if (Session["admin"] != null)
            {
                 users = db.Users.AsNoTracking().Include(u => u.Partner);
            }
            else
            {
                if (Session["userAdmin"] != null)
                {
                    ViewBag.userAdmin = true;
                    int partnerID = (int)Session["partnerID"];
                    users = db.Users.AsNoTracking().Where(x => x.partnerId == partnerID).Include(u => u.Partner);
                }
            }

            List<User> Users = users.ToList();

            return View(Users);
        }

        // GET: Users/Details/5
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
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }


            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.UserAccess(id);

            if(hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            UsLog userLog = new UsLog();

            userLog.UserLog_List = db.UserLogs.Where(x => x.userID == id);
            userLog.user = user;

            return View(userLog);
        }


        //Creates the ical file with SB and flushes it
        public ActionResult Ical(Guid s)
        {
            //var requests = db.Requests.Where((x => Encryptor.MD5Hash(x.Agreement.partnerID.ToString()) == s || Encryptor.MD5Hash(x.Agreement.Service.operatorID.ToString()) == s) && ( => b.stateID== "Submitted" || x.stateID == "Approved"));

            var user = db.Users.Where(x => x.UserIcalLink == s).FirstOrDefault();
            var userID = user.ID;

            var partner = db.Partners.Where(a =>a.ID == user.partnerId).FirstOrDefault();
            var partnerID = partner.ID;


            DateTime LimitDate = DateTime.Today.AddMonths(-1);

            var requests1 = from req in db.Requests
                            join ag in db.Agreements on req.agreementID equals ag.ID
                            join serv in db.Services on ag.serviceID equals serv.ID
                            where (req.Agreement.partnerID == partnerID || req.Agreement.Service.operatorID == partnerID) && req.ResponsibleId == userID
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
                            where (req.Agreement.partnerID == partnerID ||  req.Agreement.Service.operatorID == partnerID) && req.ResponsibleId == userID
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

            //var requests = requests1.Union(requests2).OrderBy(x => x.eventID).OrderBy(a => a.EventDate);

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


            if (index != 0)
            {

                Req_List[index].ID = eventID;
                Req_List[index].AgOp = partners;
                Req_List[index].ContactName = clientName;
                Req_List[index].NrPersons = nrPersons;
                Req_List[index].StateID = status;
                Req_List[index].Notes = notes;
            }
            var calendar = new Ical.Net.Calendar();

            string title = string.Empty;

            DateTime eventDate = new DateTime();

            TimeSpan EvDuration;

            string description = string.Empty;

            foreach (var req in Req_List)
            {
                if (req.Aglabel != null)
                {
                    title = req.Aglabel;
                }
                else
                {
                    title = req.Servlabel;
                }

                if(req.eventID != null)
                {
                    title = title + "-PAX: " + req.NrPersons;
                }
                else
                {
                    title = title + "-" + req.ContactName + "-PAX: " + req.NrPersons;
                }

                eventDate = new DateTime(req.EventDate.Value.Year, req.EventDate.Value.Month, req.EventDate.Value.Day, req.EventTime.Value.Hours, req.EventTime.Value.Minutes, req.EventTime.Value.Seconds);

                if (req.Duration == null)
                {
                    EvDuration = new TimeSpan(0, 30, 0);
                }
                else
                {
                    EvDuration = (TimeSpan)req.Duration;
                }


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


        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,username,fullName,password,email,phone,lastLoginDate,partnerId,isActive, passwordConfirm")] User user)
        {
            try
            {
                user.passwordConfirm = Encryptor.MD5Hash(user.passwordConfirm);
                user.password = Encryptor.MD5Hash(user.password);
                user.emailConfirmed = true;
                user.UserIcalLink = Guid.NewGuid();
                db.Users.Add(user);

                UserLog usLog = new UserLog();
                usLog.Date = System.DateTime.Now;
                usLog.Time = System.DateTime.Now.ToString("HH:mm");
                usLog.userID = user.ID;
                usLog.editorID = (int)Session["userID"];
                usLog.notes ="User created!";
                db.UserLogs.Add(usLog);

                db.SaveChanges();
                return RedirectToAction("Index");
            }catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Property: {0} erro: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName", user.partnerId);
            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = true;

            if (Session["userAdmin"] == null)
            {
                hasAccess = accessController.UserAccess(id);
            }

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }


            if (Session["admin"] != null || Session["userAdmin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                User user = db.Users.Find(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName", user.partnerId);
                user.password = string.Empty;

                UsLog userLog = new UsLog();

                userLog.UserLog_List = db.UserLogs.Where(x => x.userID == id);
                userLog.user = user;
                return View(userLog);
            }
            //if user is not allowed to edit is account details
            return View("Details");
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,username,fullName,password,passwordConfirm,email,phone,lastLoginDate,partnerId,isActive, isAdmin")] User user)
        {



            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            db.Configuration.LazyLoadingEnabled = true;
            db.Configuration.ProxyCreationEnabled = false;

            //db.Dispose();
            //db = new CityAccessEntities();

            // AsNoTracking is necessery due to mvc context object that would conflict with the object wished to be entered in the database
            User us = db.Users.Where(x => x.ID == user.ID).First();

            User us2 = new CityAccess.User();
            us2 = (User)UpdateHelper(us2, us);

            //Object us3 = UpdateHelper(us2, us);
            //User us4 = (User)us3;

            string password = string.Empty;
            string passwordConfirm = string.Empty;


            //if (Encryptor.MD5Hash(user.password) != us.password)
            //{
                password = Encryptor.MD5Hash(user.password);
                passwordConfirm = Encryptor.MD5Hash(user.passwordConfirm);
                us.emailConfirmed = true;
            //}

                object usr = UpdateHelper(us, user);
                us = (User)usr;
                us.password = password;
                us.passwordConfirm = passwordConfirm;
                us.emailConfirmed = true;


            Logobj Changes = Log.Changes(us2, us);

            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(us).State = EntityState.Modified;

                    UserLog usLog = new UserLog();
                    usLog.Date = System.DateTime.Now;
                    usLog.Time = System.DateTime.Now.ToString("HH:mm");
                    usLog.userID = user.ID;
                    usLog.editorID = (int)Session["userID"];
                    usLog.notes = Changes.changes;
                    db.UserLogs.Add(usLog);


                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName", user.partnerId);

            UsLog userLog = new UsLog();

            userLog.UserLog_List = db.UserLogs.Where(x => x.userID == user.ID);
            userLog.user = user;

            return View(userLog);
        }

        // GET: Users/UserEdit/5
        public ActionResult UserEdit(int? id)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.UserAccess(id);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }


            User us = db.Users.AsNoTracking().Where(x => x.ID == id).First();
            if (Session["admin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (us == null)
                {
                    return HttpNotFound();
                }
            }
            return View(us);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserEdit([Bind(Include = "ID,username,email,phone, Oldpassword, fullName, partnerId")] User user)
        {


            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            db.Configuration.LazyLoadingEnabled = true;
            db.Configuration.ProxyCreationEnabled = false;


            // AsNoTracking is necessery due to mvc context object that would conflict with the object wished to be entered in the database
            User us = db.Users.AsNoTracking().Where(x => x.ID == user.ID).First();

            if (Encryptor.MD5Hash(user.Oldpassword) != us.password)
            {
                ViewBag.passwordValidation = true;
                return View(user);
            }



            User us2 = new CityAccess.User();
            us2 = (User)UpdateHelper(us2, us);




            //user.isActive = us.isActive;
            //user.lastLoginDate = us.lastLoginDate;
            //user.emailConfirmed = us.emailConfirmed;
            //user.password = us.password;
            //user.passwordConfirm = us.password;
            //user.partnerId = us.partnerId;

            us.fullName = user.fullName;
            us.passwordConfirm = us.password;
            us.username = user.username;
            us.email = user.email;
            us.phone = user.phone;

            object usr = UpdateHelper(us, user);
            us = (User)usr;

            Logobj Changes = Log.Changes(us2, us);

            //us= user;
            //UpdateHelper(user, us);
            //try
            //{
            //    if (ModelState.IsValid)
            //    {

            db.Entry(us).State = EntityState.Modified;

                    UserLog usLog = new UserLog();
                    usLog.Date = System.DateTime.Now;
                    usLog.Time = System.DateTime.Now.ToString("HH:mm");
                    usLog.userID = user.ID;
                    usLog.editorID = (int)Session["userID"];
                    usLog.notes = Changes.changes;
                    db.UserLogs.Add(usLog);

                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = user.ID });
            //    }
            //}
            //catch (DbEntityValidationException dbEx)
            //{
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
            //        }
            //    }
            //}
            ////ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName", user.partnerId);
            //return View(user);
        }


        public ActionResult PasswordEdit(int? id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.UserAccess(id);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }


            User us = db.Users.AsNoTracking().Where(x => x.ID == id).First();
            if (Session["admin"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (us == null)
                {
                    return HttpNotFound();
                }
            }
            us.password = null;
            return View(us);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordEdit([Bind(Include = "ID,username,fullName,password,passwordConfirm,email,phone, OldPassword")] User user)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // AsNoTracking is necessery due to mvc context object that would conflict with the object wished to be entered in the database
            User us = db.Users.AsNoTracking().Where(x => x.ID == user.ID).First();

            if (Encryptor.MD5Hash(user.Oldpassword) != us.password)
            {
                ViewBag.oldpassword = true;
                return View(user);
            }
            if (user.password != null)
            {
                us.password = Encryptor.MD5Hash(user.password);
                us.passwordConfirm = Encryptor.MD5Hash(user.passwordConfirm);
            }
            else
            {
                ViewBag.newpassword = true;
                return View(user);
            }
            //try
            //{
            //    if (ModelState.IsValid)
            //    {
                    db.Entry(us).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new {id = user.ID });
            //    }
            //}
            //catch (DbEntityValidationException dbEx)
            //{
            //    foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    {
            //        foreach (var validationError in validationErrors.ValidationErrors)
            //        {
            //            System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
            //        }
            //    }
            //}
            //ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName", user.partnerId);
            //return View(user);
        }


        // GET: Users/Delete/5
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
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        public object UpdateHelper(object base_obj, object view_obj)
        {
            if (base_obj.GetType() != view_obj.GetType())
            {
                return null;
            }

            foreach (PropertyInfo propertyInfo in base_obj.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    object view_reqValue = propertyInfo.GetValue(view_obj, null);
                    if (view_reqValue != null)
                    {
                        propertyInfo.SetValue(base_obj, view_reqValue);
                    }
                }
            }
            return base_obj;
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
