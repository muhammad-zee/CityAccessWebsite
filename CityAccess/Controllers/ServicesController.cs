using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI.WebControls;
using System.Web.UI;
using CityAccess.Controllers;
using System.Globalization;


namespace CityAccess
{
    public class ServicesController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Services
        public ActionResult Index()
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            IQueryable<Service> queryable = null;
            var services  = queryable;

            if(TempData["Ag"] != null)
            {
                ViewBag.Ag = 1;
            }


            if (Session["admin"] == null)
            {
                int partnerID = (int)Session["partnerID"];
                services = db.Services.Where(x => x.operatorID == partnerID);
               
            }else
            {
               services = db.Services.Include(s => s.City).Include(s => s.Partner).Include(s => s.serviceType);
            }
            return View(services.ToList());
        }

        // GET: Services/Details/5
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
            Service service = db.Services.Find(id);

            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(service.Partner.ID);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }



            DynamicFieldAlternative dFA = new DynamicFieldAlternative();

            dFA =  db.DynamicFieldAlternatives.Find(service.comissionType);
            service.commissionTypeLabel = dFA.label;
            dFA  = db.DynamicFieldAlternatives.Find(service.priceType);
            service.priceTypeLabel = dFA.label;
            dFA  = db.DynamicFieldAlternatives.Find(service.PaymentAgentType);
            service.PaymentAgentTypeLabel = dFA.label;
            dFA = db.DynamicFieldAlternatives.Find(service.Availability1);
            service.AvailabilityLabel = dFA.label;
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        // GET: Services/Create
        public ActionResult Create()
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            //Search by ID, if it were by label conflicts would arise
            int priceID = 3;
            int commissionID = 2;
            int paymentAgentID = 1;
            int availabilityID = 4;

            ViewBag.price = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == priceID);
            ViewBag.commission = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == commissionID);
            ViewBag.paymentAgent = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == paymentAgentID);
            ViewBag.availability = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == availabilityID);


            ViewBag.cityID = new SelectList(db.Cities, "ID", "name");
            ViewBag.operatorID = new SelectList(db.Partners, "ID", "tradeName");
            ViewBag.typeID = new SelectList(db.serviceTypes, "ID", "name");
            return View();
        }

        // POST: Services/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,typeID,name,description,cityID,isPublic, isActive, operatorID, file, fieldName1, fieldName2, fieldName3, fieldName4, fieldName5, fieldName6, fieldName7, fieldName8, fieldName9, fieldName10, fieldName11, fieldName12, field1IsActive, field2IsActive, field3IsActive, field4IsActive, field5IsActive, field6IsActive, field7IsActive, field8IsActive, field9IsActive, field10IsActive, field11IsActive, field12IsActive, field1IsMandatory, field2IsMandatory, field3IsMandatory, field4IsMandatory, field5IsMandatory, field6IsMandatory, field7IsMandatory, field8IsMandatory, field9IsMandatory, field10IsMandatory, field11IsMandatory, field12IsMandatory, price, priceType, commissionValue, comissionType, PaymentAgent, PaymentAgentType, agentInstructions, ConfirmationText, cancellationPolicy, MaxPersonNum, MinPersonNum, Override1, Availability1, Duration")] Service service)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            if (Session["admin"] == null)
            {
                service.operatorID = (int)Session["partnerID"];
            }



                ServiceImage img = new ServiceImage();
            if (service.file != null)
            {
                //image resize so they all have the same size 
                WebImage imgResized = new WebImage(service.file.InputStream);

                float ratio = (float)imgResized.Width / (float)imgResized.Height;

                if (ratio > 2)
                {
                    float w = 125 * ratio;
                    
                    imgResized = imgResized.Resize((int)w, 125);
                    int sizeToCrop = (imgResized.Width - 250)/2;
                    imgResized = imgResized.Crop(0, sizeToCrop , 0, sizeToCrop );
                }
                else
                {

                    float h = 250 / ratio;
                    imgResized = imgResized.Resize(250,(int)h);
                    int sizeToCrop = (imgResized.Height - 125)/2;
                    imgResized = imgResized.Crop(sizeToCrop, 0, sizeToCrop, 0);
                }

                img.Image = imgResized.GetBytes();
                    // img.Image = ConvertToBytes(imgResized);
            }

            if (ModelState.IsValid)
            {
                // comes before and needs the saveChanges call to not auto-populate the ID field
                db.Services.Add(service);
                db.SaveChanges();


                if (service.file != null) // for providers to have the option of adding the image later
                {
                    try
                    {
                        Service serv = db.Services.Find(service.ID);
                        img.serviceID = serv.ID;
                        var servImg = db.ServiceImages.Where(a => a.serviceID == service.ID);
                        img.sequenceNR = servImg.Count() + 1;

                        db.ServiceImages.Add(img);
                        db.SaveChanges();

                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var url = "Services/Create";
                        var userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);
                    
                        return RedirectToAction("Index", "Error");
                    }
                }


                return RedirectToAction("Index");
            }

            ViewBag.cityID = new SelectList(db.Cities, "ID", "name", service.cityID);
            ViewBag.operatorID = new SelectList(db.Partners, "ID", "tradeName", service.operatorID);
            ViewBag.typeID = new SelectList(db.serviceTypes, "ID", "name", service.typeID);
            return View(service);
        }

        // GET: Services/Edit/5
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
            Service service = db.Services.Find(id);




            if (service == null)
            {
                return HttpNotFound();
            }


            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(service.Partner.ID);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            var servImg = db.ServiceImages.Where(a => a.serviceID == service.ID);
            ViewBag.Imgs = servImg;
            ViewBag.cityID = new SelectList(db.Cities, "ID", "name", service.cityID);
            ViewBag.operatorID = new SelectList(db.Partners, "ID", "tradeName", service.operatorID);
            ViewBag.typeID = new SelectList(db.serviceTypes, "ID", "name", service.typeID);


            //Search by ID, if it were by label conflicts would arise
            int priceID = 3;
            int commissionID = 2;
            int paymentAgentID = 1;
            int availabilityID = 4;


            ViewBag.price = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == priceID);
            ViewBag.commission = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == commissionID);
            ViewBag.paymentAgent = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == paymentAgentID);
            ViewBag.availability = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == availabilityID);
            return View(service);
        }

        // POST: Services/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "save")]
        public ActionResult Edit([Bind(Include = "ID,typeID,name,description,cityID,isPublic, isActive, operatorID, selectedID, file, fieldName1, fieldName2, fieldName3, fieldName4, fieldName5, fieldName6, fieldName7, fieldName8, fieldName9, fieldName10, fieldName11, fieldName12, field1IsActive, field2IsActive, field3IsActive, field4IsActive, field5IsActive, field6IsActive, field7IsActive, field8IsActive, field9IsActive, field10IsActive, field11IsActive, field12IsActive, field1IsMandatory, field2IsMandatory, field3IsMandatory, field4IsMandatory, field5IsMandatory, field6IsMandatory, field7IsMandatory, field8IsMandatory, field9IsMandatory, field10IsMandatory, field11IsMandatory, field12IsMandatory, MaxPersonNum, MinPersonNum, Override1, PaymentAgent, agentInstructions, ConfirmationText, cancellationPolicy, commissionValue, price, priceType, comissionType, PaymentAgentType, Availability1, Duration")] Service service)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var servImg = db.ServiceImages.Where(a => a.serviceID == service.ID);

            if (servImg.Any())
            {
                var servCurrentImage = db.ServiceImages.Where(b => b.sequenceNR == 1 && b.serviceID == service.ID).First();
            }

            if (Session["admin"] == null)
            {
                service.operatorID = (int)Session["partnerID"];
            }


            //Pushing current image down in the sequence
            if (service.selectedID != null)
            {
                int i = 0;
                int k = 0;
                string aux;

                aux = service.selectedID.Remove(0, 8);
                for (i = 0; i < aux.Length; i++)
                {
                    if (aux[i] == '/')
                    {
                        k++;
                    }
                    if (k > 0)
                    {
                        aux = aux.Remove(i, aux.Length - i);
                        i = aux.Length;
                    }
                }

                k = Int32.Parse(aux);
                ServiceImage num1img = db.ServiceImages.Where(b => b.sequenceNR == 1).Where(b => b.serviceID == service.ID).FirstOrDefault();
                ServiceImage new_num1img = db.ServiceImages.Find(k);

                num1img.sequenceNR = new_num1img.sequenceNR;
                new_num1img.sequenceNR = 1;

                db.Entry(num1img).State = EntityState.Modified;
                db.Entry(new_num1img).State = EntityState.Modified;
            }

           // if (ModelState.IsValid)
           // {
                try
                {
                if (service.file != null)
                {
                        ServiceImage img = new ServiceImage();
                        WebImage imgResized = new WebImage(service.file.InputStream);

                        float ratio = (float)imgResized.Width / (float)imgResized.Height;

                        if (ratio > 2)
                        {
                            float w = 125 * ratio;

                            imgResized = imgResized.Resize((int)w, 125);
                            int sizeToCrop = (imgResized.Width - 250) / 2;
                            imgResized = imgResized.Crop(0, sizeToCrop, 0, sizeToCrop);
                        }
                        else
                        {

                            float h = 250 / ratio;
                            imgResized = imgResized.Resize(250, (int)h);
                            int sizeToCrop = (imgResized.Height - 125) / 2;
                            imgResized = imgResized.Crop(sizeToCrop, 0, sizeToCrop, 0);
                        }
                        img.Image = imgResized.GetBytes();
                        img.serviceID = service.ID;


                        img.sequenceNR = servImg.Count() + 1;
                        db.ServiceImages.Add(img);

                    }



                    db.Entry(service).State = EntityState.Modified;
                    db.SaveChanges();

                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "Services/Edit";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);
            
                    return RedirectToAction("Index", "Error");
                }

                return RedirectToAction("Index");
        }

        [HttpPost]
        [MultipleButton(Name ="action", Argument ="RemoveImg")]
        public ActionResult RemoveImg([Bind(Include = "ID,typeID,name,description,cityID,isPublic,operatorID, file, selectedID")] Service service)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            IQueryable<Service> queryable = null;
            var services = queryable;

            if (Session["admin"] == null)
            {
                int partnerID = (int)Session["partnerID"];
                services = db.Services.Where(x => x.operatorID == partnerID);

            }
            else
            {
                services = db.Services.Include(s => s.City).Include(s => s.Partner).Include(s => s.serviceType);
            }


            if (service.selectedID != null)
            {
                int i = 0;
                int k = 0;
                string aux;

                aux = service.selectedID.Remove(0, 8);
                for (i = 0; i < aux.Length; i++)
                {
                    if (aux[i] == '/')
                    {
                        k++;
                    }
                    if (k > 0)
                    {
                        aux = aux.Remove(i, aux.Length - i);
                        i = aux.Length;
                    }
                }

                k = Int32.Parse(aux);
                ServiceImage num1img = db.ServiceImages.Where(b => b.sequenceNR == 1).Where(b => b.serviceID == service.ID).FirstOrDefault();
                ServiceImage new_num1img = db.ServiceImages.Find(k);

                if(new_num1img.sequenceNR == 1)
                {
                    num1img = db.ServiceImages.Where(b => b.sequenceNR > 1).Where(b => b.serviceID == service.ID).FirstOrDefault();
                    if (num1img != null)
                    {
                        num1img.sequenceNR = 1;
                    }
                }


                try
                {
                    db.ServiceImages.Remove(new_num1img);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "Services/RemoveImg";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);
                
                    return RedirectToAction("Index", "Error");
                }
            }

            return RedirectToAction("Index", services.ToList());
        }


        // GET: Services/Delete/5
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
            Service service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }

            AccessController accessController = new AccessController();
            accessController.Initialize(this.Request.RequestContext);

            Boolean hasAccess = accessController.PartnerAccess(service.Partner.ID);

            if (hasAccess == false)
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            IQueryable<ServiceImage> servImg = db.ServiceImages.Where(a => a.serviceID == id);
            foreach(ServiceImage item in servImg)
            {
                db.ServiceImages.Remove(item);
            }

            var ag = db.Agreements.Where(x => x.serviceID == id);


            try
            {
                if (ag == null)
                {
                    Service service = db.Services.Find(id);
                    db.Services.Remove(service);
                    db.SaveChanges();
                }else
                {
                    TempData["Ag"] = "MyMessage";
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Services/DeleteConfirmed";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            
                return RedirectToAction("Index", "Error");
            }

            return RedirectToAction("Index");
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


        /// <summary>
        /// Post to create events or alter the notes or the status
        /// </summary>
        /// <param name="Mlevnt"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Event(MultipleEvent Mlevnt)
        {

            var events = db.Events.Where(x => x.serviceID == Mlevnt.serviceID && x.eventDate >= System.DateTime.Today).OrderBy(a => a.eventDate + " " + a.startTime);

            //used for updating the DB
            Event ev = new Event();

            //logic for adding of events 


            if(Mlevnt.frequency != null)
            {

                /////// One time events logic /////////////////////////////////////////7
                if (Mlevnt.frequency == "Once")
                {
                    ev.serviceID = Mlevnt.serviceID;
                    ev.eventDate = Mlevnt.startDate;
                    ev.startTime = Mlevnt.startTime;
                    ev.endTime= Mlevnt.endTime;
                    ev.stateID = "open";
                    ev.maxPersons = Mlevnt.maxPersons;
                    ev.notes = Mlevnt.notes;

                    try
                    {
                        db.Events.Add(ev);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var url = "Services/Event";
                        var userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);
                    
                        return RedirectToAction("Index", "Error");
                    }
                }


                var currentCulture = CultureInfo.CurrentCulture;
                string day = currentCulture.Calendar.GetDayOfWeek(System.DateTime.Today).ToString();





                // Daily events logic /////////////////////////////////////////////
                if (Mlevnt.frequency == "Daily")
                {
                    DateTime BaseDate = Mlevnt.startDate;  
                    string WeekBaseDate = currentCulture.Calendar.GetDayOfWeek(Mlevnt.startDate).ToString();

                    DateTime iteratingDate = BaseDate;
                    string dayOfweek;
                    int i = 0;
                    DateTime[] dateArray = new DateTime[7];

                    //Fetching the multiple dates for the event scheduling
                    for(int k = 0; k < 7; iteratingDate = iteratingDate.AddDays(1))
                    {
                        dayOfweek = currentCulture.Calendar.GetDayOfWeek(iteratingDate).ToString();

                       if (Mlevnt.Monday == true && dayOfweek == "Monday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        if (Mlevnt.Tuesday == true && dayOfweek == "Tuesday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        if (Mlevnt.Wednesday == true && dayOfweek == "Wednesday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        if (Mlevnt.Thursday == true && dayOfweek == "Thursday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        if (Mlevnt.Friday == true && dayOfweek == "Friday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        if (Mlevnt.Saturday == true && dayOfweek == "Saturday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }


                        if (Mlevnt.Sunday == true && dayOfweek == "Sunday")
                        {
                            dateArray[i] = iteratingDate;
                            i++;
                        }

                        k++;
                    }

                    //Checking the bigger date
                    var biggerDate = dateArray[0];
                    var limitDate = Mlevnt.endDate;

                    for (i = 1; i < 7; i++)
                    {
                        if(dateArray[i] != null && dateArray[i] > biggerDate)
                        {
                            biggerDate = dateArray[i];
                        }
                    }


                    //Logic for adding 

                    ev.serviceID = Mlevnt.serviceID;
                    ev.startTime = Mlevnt.startTime;
                    ev.endTime = Mlevnt.endTime;
                    ev.stateID = "open";
                    ev.maxPersons = Mlevnt.maxPersons;
                    ev.notes = Mlevnt.notes;

                    for (; biggerDate <= limitDate; biggerDate = biggerDate.AddDays(7))
                    {

                        for (int v = 0; v < 7; v++)
                        {

                            if (dateArray[v] != System.DateTime.MinValue)
                            {
                                ev.eventDate = dateArray[v];
                                dateArray[v] = dateArray[v].AddDays(7);

                                try
                                {
                                    db.Events.Add(ev);
                                    db.SaveChanges();
                                }
                                catch (DbEntityValidationException dbEx)
                                {
                                    var url = "Services/Event";
                                    var userID = (int)Session["userID"];

                                    auxMethods.ErrorHandling(dbEx, url, userID);
                                
                                    return RedirectToAction("Index", "Error");
                                }
                            }
                        }
                    }

                    //Last iteration of the cycle , because the loop above stops by maximum date
                    for (int v = 0; v < 7; v++)
                    {

                        if (dateArray[v] != null && dateArray[v] <= limitDate)
                        {
                            ev.eventDate = dateArray[v];


                            try
                            {
                                db.Events.Add(ev);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException dbEx)
                            {
                                var url = "Services/Event";
                                var userID = (int)Session["userID"];

                                auxMethods.ErrorHandling(dbEx, url, userID);
                            
                                return RedirectToAction("Index", "Error");
                            }
                        }
                    }


                }



                ///////////////////  Weekly events logic ////////////////////////////////////
                if (Mlevnt.frequency == "Weekly")
                {
                    var startdate = Mlevnt.startDate;
                    var limitDate = Mlevnt.endDate;

                    ev.serviceID = Mlevnt.serviceID;
                    ev.startTime = Mlevnt.startTime;
                    ev.endTime = Mlevnt.endTime;
                    ev.stateID = "open";
                    ev.maxPersons = Mlevnt.maxPersons;
                    ev.notes = Mlevnt.notes;


                    for (var i = startdate; i <= limitDate; i = i.AddDays(7))
                    {

                        ev.eventDate = i;

                        try
                        {
                            db.Events.Add(ev);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "Services/Event";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);

                            return RedirectToAction("Index", "Error");
                        }
                    }
                }



                //////////////////// Monthly events logic //////////////////////////////
                if (Mlevnt.frequency == "Monthly")
                {
                    var startdate = Mlevnt.startDate;
                    var limitDate = Mlevnt.endDate;

                    ev.serviceID = Mlevnt.serviceID;
                    ev.startTime = Mlevnt.startTime;
                    ev.endTime = Mlevnt.endTime;
                    ev.stateID = "open";
                    ev.maxPersons = Mlevnt.maxPersons;


                    for (var i = startdate; i <= limitDate; i = i.AddMonths(1))
                    {

                        ev.eventDate = i;

                        try
                        {
                            db.Events.Add(ev);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "Services/Event";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);

                            return RedirectToAction("Index", "Error");
                        }
                    }
                }
            }



            ////////// Second part of post that handles edits in event status and event notes ////////////////////////////////////
            //check for changes in events notes and status
            if (Mlevnt.Events != null)
            {


                int limit = Mlevnt.Events.Count();

                List<Event> eventsList = events.ToList();

                string emailAddress = string.Empty;

                string subject = string.Empty;

                string content = string.Empty;

                EmailController email = new EmailController();

                string eventDetails = string.Empty;

                Event evList = new Event();

                string og = string.Empty;



                for (int i = 0; i < limit;  i++ )
                {
                    ev = Mlevnt.Events.ElementAt(i);

                    evList = eventsList.ElementAt(i);

                    if (ev.notes != evList.notes || ev.stateID != evList.stateID)
                    {

                        content = "<br/><br> Event from " + evList.Service.Partner.tradeName + " was changed:<br/><br>" + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                                  "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody>";

                        if (ev.notes != evList.notes)
                        {

                            content = content + "<tr><td>Notes</td><td>" + evList.notes + "</td><td>" + ev.notes + "</td></tr>";
                            evList.notes = ev.notes;
                        }

                        if (ev.stateID != evList.stateID)
                        {
                            content = content + "<tr><td>Status</td><td>" + evList.stateID + "</td><td>" + ev.stateID + "</td></tr>";
                            evList.stateID = ev.stateID;
                        }



                        var req4part = db.Requests.Where(x => x.eventID == ev.ID).GroupBy(a => a.Agreement.Partner).Select(g => g.FirstOrDefault());


                        if (req4part.Count() > 0)
                        {
                            content = content + "</tbody></table>";

                            og = " " + evList.eventDate.ToString("dd-MM-yyyy");

                            eventDetails = "<br/><br><p></p>Event general details:<br/><br> " + "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>" +
                            "<tr><th>Field</th><th>Value</th></tr></thead>" +
                            "<tbody  style='line-height: 2;font-size: 12px;'>" +
                            "<tr><td>Service</td><td>" + evList.Service.name + "</td></tr>"
                            + "<tr><td> Day</td><td> " + og + " </td></tr>"
                             + "<tr><td> Start Time </td><td> " + evList.startTime + " </td></tr>"
                             + "<tr><td> End Time </td><td> " + evList.endTime + " </td></tr>" +
                             "<tr><td>Max Persons</td><td>" + evList.maxPersons + "</td></tr></tbody></table>";

                            subject = "Event#" + ev.ID + " from Service -" + req4part.First().Agreement.Service.name + " was changed.";

                            content = content + eventDetails;

                            foreach (var part in req4part)
                            {
                                emailAddress = part.Agreement.Partner.ContactEmail;
                                email.Email_to_send(emailAddress, string.Empty, content, subject);
                            }
                        }
                        try
                        {

                            db.Entry(evList).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "Services/Event";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);

                            return RedirectToAction("Index", "Error");
                        }
                    }
                }
            }



            ///// Setup for the Model to be feeded to the view
            events = db.Events.Where(x => x.serviceID == Mlevnt.serviceID && x.eventDate >= System.DateTime.Today).OrderBy(a => a.eventDate + " " + a.startTime);
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
            mltEvent.Events = events.ToList();
            mltEvent.startDate = Mlevnt.startDate;
            mltEvent.endDate = Mlevnt.endDate;

            ViewBag.stateID = db.EventStates;

            return View(mltEvent);
        }


        /// <summary>
        ///  Deletes the selected event 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="servID"></param>
        /// <returns></returns>
        [HttpPost]
        public Boolean DeleteEvent(int? id, int servID)
        {
            try
            {
                Event ev = db.Events.Find(id);
                db.Events.Remove(ev);
                db.SaveChanges();
            }

            catch (DbEntityValidationException dbEx)
            {
                var url = "Services/DeleteEvent";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return false;
            }
            return true;
            //return Event(servID);
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public byte[] ConvertToBytes(HttpPostedFileBase image)

        {

            byte[] imageBytes = null;

            BinaryReader reader = new BinaryReader(image.InputStream);

            imageBytes = reader.ReadBytes((int)image.ContentLength);

            return imageBytes;

        }
    }
}
