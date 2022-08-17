using CityAccess.Controllers;
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

namespace CityAccess
{
    public class AgreementsController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Agreements
        /// <summary>
        //  Querys de DB to present lis of agreements to a logged in user
        /// </summary>
        /// <param name="SearchString">to search by present words in agreement title</param>
        /// <param name="AgreementsService"></param>
        /// <param name="agr"></param>
        /// <param name="operator1">string to filter agreements by operator</param>
        /// <param name="agent">string to filter agreements by agent</param>
        /// <param name="partnerID">for external access through partner site</param>
        /// <returns>the view that displays a list of agreements on screen</returns>
        public ActionResult Index(string SearchString, string AgreementsService, bool? agr,  string operator1, string agent, int? partnerID)
        {
            //var agreements = db.Agreements.Include(a => a.commissionType1).Include(a => a.Partner).Include(a => a.Service);
            int? value = null;
            string baseService = "Base Service";

            IQueryable<CityAccess.Agr_Partn_Comm> queryable = null;
            var agreements = queryable;
            var agreements1 = queryable;

            var origin = false;

            // ViewBag.images = db.ServiceImages.Where(g => g.sequenceNR == 1).OrderBy(g =>g.Service.Agreements);

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null && partnerID == null)
                {
                    return RedirectToAction("Index", "Login");
                }


                if (partnerID == null)
                {
                    partnerID = (int)Session["partnerID"];
                }
                else
                {
                    origin = true;
                }

                ViewBag.Services = (from ag in db.Agreements
                                    join serv in db.Services on ag.serviceID equals serv.ID
                                    where serv.operatorID == partnerID || ag.partnerID == null || ag.partnerID == partnerID
                                    select serv).Distinct();

                ViewBag.operators = (from ag in db.Agreements
                                     join serv in db.Services on ag.serviceID equals serv.ID
                                     join part in db.Partners on serv.operatorID equals part.ID
                                     where ag.partnerID == partnerID
                                     select part).Distinct();

                ViewBag.partners = (from ag in db.Agreements
                                    join serv in db.Services on ag.serviceID equals serv.ID
                                    join part in db.Partners on ag.partnerID equals part.ID
                                    where serv.operatorID == partnerID
                                    select part).Distinct();

                Partner partner = db.Partners.Where(x => x.ID == partnerID).First();

                if (partner.isAgent == true)
                {
                    ViewBag.isAgent = 1;
                }
                if (partner.isOperator == true)
                {
                    ViewBag.isOperator = 1;
                }


                //Reformular as querys a imagem dos servicesBooked e dos servicesRequested
                    if (partner.isAgent == true && partner.isOperator != true)
                    {
                        if (db.PartnerLogoes.Where(x => x.PartnerID == partnerID).Any())
                        {
                            agreements =
                                from ag in db.Agreements
                                join serv in db.Services on ag.serviceID equals serv.ID
                                join partn in db.Partners on serv.operatorID equals partn.ID
                                join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                //join partLogo in db.PartnerLogoes on ag.partnerID equals partLogo.PartnerID
                                where servImg.sequenceNR == 1
                                where serv.isActive == true
                                where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value)

                                select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg/*, PartnerLogo = partLogo*/ };

                                 agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value)

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                        }
                        else
                        {
                            agreements =
                                from ag in db.Agreements
                                join serv in db.Services on ag.serviceID equals serv.ID
                                join partn in db.Partners on serv.operatorID equals partn.ID
                                join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                where servImg.sequenceNR == 1
                                where serv.isActive == true
                                where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value)

                                select new Agr_Partn_Comm {  Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg };

                            agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value)

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                        }
                    }
                    else
                    {
                        if (partner.isAgent == true && partner.isOperator == true)
                        {
                            if (db.PartnerLogoes.Where(x => x.PartnerID == partnerID).Any())
                            {
                                agreements =
                                    from ag in db.Agreements
                                    join serv in db.Services on ag.serviceID equals serv.ID
                                    join partn in db.Partners on serv.operatorID equals partn.ID
                                    join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                    //join partLogo in db.PartnerLogoes on ag.partnerID equals partLogo.PartnerID
                                    where servImg.sequenceNR == 1
                                    where serv.isActive == true
                                    where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value) || (serv.operatorID == partnerID && ag.partnerID != null && ag.partnerID != serv.operatorID)

                                    select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg, /*PartnerLogo = partLogo*/ };

                                agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value) || (serv.operatorID == partnerID && ag.partnerID != null && ag.partnerID != serv.operatorID)

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                            }
                            else
                            {
                                agreements =
                                    from ag in db.Agreements
                                    join serv in db.Services on ag.serviceID equals serv.ID
                                    join partn in db.Partners on serv.operatorID equals partn.ID
                                    join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                    where servImg.sequenceNR == 1
                                    where serv.isActive == true
                                    where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value) || (serv.operatorID == partnerID && ag.partnerID != null && ag.partnerID != serv.operatorID)

                                    select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg };


                                agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where ag.partnerID == partnerID || (value == null ? ag.partnerID == null : ag.partnerID == value) || (serv.operatorID == partnerID && ag.partnerID != null && ag.partnerID != serv.operatorID)

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                            }
                        }
                    }
                    if (partner.isAgent != true && partner.isOperator == true)
                    {
                        if (db.PartnerLogoes.Where(x => x.PartnerID == partnerID).Any())
                        {
                            agreements =
                                from ag in db.Agreements
                                join serv in db.Services on ag.serviceID equals serv.ID
                                join partn in db.Partners on serv.operatorID equals partn.ID
                                join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                //join partLogo in db.PartnerLogoes on ag.partnerID equals partLogo.PartnerID
                                where servImg.sequenceNR == 1
                                where serv.isActive == true
                                where  serv.operatorID == partnerID

                                select new Agr_Partn_Comm {  Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg/*, PartnerLogo = partLogo */};


                             agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where serv.operatorID == partnerID

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                        }
                        else
                        {
                            agreements =
                                from ag in db.Agreements
                                join serv in db.Services on ag.serviceID equals serv.ID
                                join partn in db.Partners on serv.operatorID equals partn.ID
                                join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                                where servImg.sequenceNR == 1
                                where serv.isActive == true
                                where  serv.operatorID == partnerID

                                select new Agr_Partn_Comm {  Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg };

                             agreements1 =
                                        from ag in db.Agreements
                                        join serv in db.Services on ag.serviceID equals serv.ID
                                        join partn in db.Partners on serv.operatorID equals partn.ID
                                        where serv.isActive == true &&  (value == null ? serv.ServiceImages.FirstOrDefault().Image == null : serv.ServiceImages.FirstOrDefault().ID == value)
                                        where serv.operatorID == partnerID

                                        select new Agr_Partn_Comm { Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = null};

                                agreements = agreements.Union(agreements1);
                        }
                    }
                


                if (agent != null && agent != "")
                {
                    agreements = agreements.Where(x => x.Agreement.Partner.tradeName == agent);
                }
                if (operator1 != null && operator1 != "")
                {
                    agreements = agreements.Where(x => x.Partner.tradeName == operator1);
                }
                if(agr == true)
                {
                agreements = agreements.Where(x => x.Agreement.isConfirmed == false || x.Agreement.isConfirmed == null);
                ViewBag.NonConfirmed = 1;
                }
                else
                {
                agreements = agreements.Where(x => x.Agreement.isConfirmed == true);
                }


                if (!String.IsNullOrEmpty(SearchString))
                {
                    agreements = agreements.Where(s => s.Agreement.label.Contains(SearchString) || s.Agreement.description.Contains(SearchString));
                }

                if (!String.IsNullOrEmpty(AgreementsService))
                {
                    agreements = agreements.Where(x => x.Agreement.Service.name == AgreementsService);
                }


                agreements = agreements.Where(x => x.Agreement.isActive == true || x.Agreement.Service.operatorID == partnerID);
                //agreements = agreements.Where(x => x.Agreement.isActive == true);




                var a = agreements.ToList();
                int[] xk = new int[(int)a.LongCount()];
                int i = 0;
                int z = 0;
                int length;
                //temporary solution, this way it takes 2 db accesses when it should only take on 
                foreach (Agr_Partn_Comm agrP in a)
                {
                    if(agrP.Agreement.partnerID != null)
                    {
                        agrP.PartnerLogo = db.PartnerLogoes.Where(x => x.PartnerID == agrP.Agreement.partnerID).FirstOrDefault();
                    }
                    if(agrP.Agreement.label == null)
                    {
                        agrP.Label = agrP.Agreement.Service.name;
                    }
                    else
                    {
                        agrP.Label = agrP.Agreement.label;
                    }

                    length = agrP.Label.Length;
                    
                    if (length > 25) { 
                        for (int j = 0; j < length; j++)
                        {
                            if(agrP.Label[j] == ' ')
                            {
                                z++;    
                            }
                        }
                        if ((float)(length / z) >= 9 || z >= 4)
                        {
                            xk[i] = agrP.Agreement.ID;
                            i++;
                        }
                    }
                }


                ViewBag.AgIds = xk;
                if (!origin)
                {
                    return View(a);
                }
                else
                {
                    return View("PartnerSiteIndex", a);
                }
            }
            else
            {
                agreements =
                    from ag in db.Agreements
                    join serv in db.Services on ag.serviceID equals serv.ID
                    join partn in db.Partners on serv.operatorID equals partn.ID
                    //join commission in db.commissionTypes on ag.commissionType equals commission.ID
                    join servImg in db.ServiceImages on serv.ID equals servImg.serviceID
                    where servImg.sequenceNR == 1
                    where ag.ID >= 1 || (value == null ? ag.partnerID == null : ag.partnerID == value)

                    select new Agr_Partn_Comm { /*Commission = commission,*/ Agreement = ag, Partner = partn, BaseService = baseService, serviceImage = servImg };


                if (!String.IsNullOrEmpty(SearchString))
                {
                    agreements = agreements.Where(s => s.Agreement.label.Contains(SearchString) || s.Agreement.description.Contains(SearchString));
                }

                if (!String.IsNullOrEmpty(AgreementsService))
                {
                    agreements = agreements.Where(x => x.Agreement.Service.name == AgreementsService);
                }

                ViewBag.services = db.Services;

                var b = agreements.ToList();
                int[] xk = new int[(int)b.LongCount()];
                int i = 0;
                int z = 0;
                int length;
                //temporary solution, this way it takes 2 db accesses when it should only take on 
                foreach (Agr_Partn_Comm agrP in b)
                {
                    if (agrP.Agreement.partnerID != null)
                    {
                        agrP.PartnerLogo = db.PartnerLogoes.Where(x => x.PartnerID == agrP.Agreement.partnerID).FirstOrDefault();
                    }
                    if (agrP.Agreement.label == null)
                    {
                        agrP.Label = agrP.Agreement.Service.name;
                    }
                    else
                    {
                        agrP.Label = agrP.Agreement.label;
                    }

                    length = agrP.Label.Length;

                    if (length > 25)
                    {
                        for (int j = 0; j < length; j++)
                        {
                            if (agrP.Label[j] == ' ')
                            {
                                z++;
                            }
                        }
                        if ((float)(length / z) >= 9 || z >= 4)
                        {
                            xk[i] = agrP.Agreement.ID;
                            i++;
                        }
                    }
                }

                

                ViewBag.AgIds = xk;
                ViewBag.Json = Json(b);
                if (!origin)
                {
                    return View(b);
                }
                else
                {
                    return View("PartnerSiteIndex", b);
                }
            }
            return HttpNotFound();
        }

        // GET: Agreements/Details/5
        /// <summary>
        /// Querys the DB to pass model to view with the agreement details
        /// </summary>
        /// <param name="id">id of the agreement which we want to present details</param>
        /// <returns>the view of the agreement details</returns>
        public ActionResult Details(int id)
        {

            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            Agreement ag = db.Agreements.Find(id);
            if (ag == null)
            {
                return HttpNotFound();
            }

            var serv = db.Services.Where(x => x.ID == ag.serviceID).FirstOrDefault();
            var servType = db.serviceTypes.Where(w => w.ID == serv.typeID).FirstOrDefault();
            var servImg = db.ServiceImages.Where(j => j.serviceID == serv.ID && j.sequenceNR == 1).FirstOrDefault();

            Req_User agr = AgreementDetails(id);
            agr.Agreement = ag;
            agr.serviceImage = servImg;

            int partnerID = (int)Session["partnerID"];

            if (ag.partnerID == partnerID)
            {
                ViewBag.Agent = true;
            }
            agr.isConfirmed = agr.Agreement.isConfirmed;

            return View(agr);
        }


        // GET: Agreements/Details/5
        /// <summary>
        /// post to confirm agreement 
        /// </summary>
        /// <param name="ag">has the ID of the agreement to be confirmed</param>
        /// <param name="isConfirmed">boolean var: true if agreement is confirmed - false if agreement is not confirmed</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details ([Bind(Include = "ID")] Agreement ag, Boolean isConfirmed)
        {
            Agreement agreement = db.Agreements.Find(ag.ID);
            agreement.isConfirmed = isConfirmed;

            db.Entry(agreement).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Agreements");
        }

        // GET: Agreements/Create
        /// <summary>
        /// sets the view to create a new agreement
        /// </summary>
        /// <returns> the view to create a new agreement</returns>
        public ActionResult Create()
        {


            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.commissionType = new SelectList(db.commissionTypes, "ID", "label");

            int partnerID = (int)Session["partnerID"];
            Partner part = db.Partners.Find(partnerID);

            if (part.isTest != true)
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest != true && a.isAgent == true), "ID", "tradeName");
            }
            else
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest == true && a.isAgent == true), "ID", "tradeName");
            }
            if (Session["admin"] != null)
            {
                ViewBag.serviceID = new SelectList(db.Services, "ID", "name");
            }else
            {
                ViewBag.serviceID = new SelectList(db.Services.Where(x => x.operatorID == partnerID),"ID","name");
            }

            int priceID = 3;
            int commissionID = 2;
            int paymentAgentID = 1;
            int availabilityID = 4;

            ViewBag.price = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == priceID);
            ViewBag.commission = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == commissionID);
            ViewBag.paymentAgent = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == paymentAgentID);
            ViewBag.availability = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == availabilityID);


            return View();
        }

        // POST: Agreements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// post to the DB, to effectivly create the agreement, also sends email to agent notifying the agreement creation
        /// </summary>
        /// <param name="agreement"> agreement to post to the DB</param>
        /// <returns>if created with success returns the agreement index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,partnerID,serviceID,label,description,isActive,messageTemplate,agentInstructions,cancellationPolicy,needsApproval,price, commissionValue, priceType, TypeCommission, PaymentAgent, PaymentAgentType, Override1 ")] Agreement agreement)
        {
            try
            {
                //if(agreement.label == null)
                //{
                //    agreement.label = db.Services.Where(x => x.ID == agreement.serviceID).First().name;
                //}


                if (ModelState.IsValid)
                {
                    db.Agreements.Add(agreement);
                    db.SaveChanges();

                    //Agreement creation e-mail

                    Partner agent = db.Partners.Where(a=> a.ID == agreement.partnerID).First();
                    Service serv = db.Services.Where(x => x.ID == agreement.serviceID).First();
                    Partner operator1 = db.Partners.Where(a => a.ID == serv.operatorID).First();

                    string time1 = System.DateTime.Now.ToString("HH:mm");
                    EmailController email = new EmailController();
                    var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, "/");

                    string url1 = "Agreements/Confirm/" + agreement.ID;
                    url1 = link + url1;

                    string subject = "New Agreement by " + operator1.tradeName + " needs confirmation";
                    string content = "Agreement details:<br/><br>"+
                         "<table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                        + "<tr><th>Field</th><th>Value</th></tr></thead><tbody>" +
                        "<tr><td> Agreement name </td><td> " + agreement.label + " </td></tr>" +
                        "<tr><td> Service name </td><td> " + serv.name + " </td></tr>" +
                        "<tr><td> Agent </td><td> " + agent.tradeName + " </td></tr>" +
                        "<tr><td> Operator </td><td> " + operator1.tradeName + " </td></tr>" +
                        "<tr><td> Confirmation text </td><td> " + agreement.messageTemplate + " </td></tr>" +
                        "<tr><td> Agent instructions </td><td> " + agreement.agentInstructions + " </td></tr>" +
                        "<tr><td> Cancellation policy </td><td> " + agreement.cancellationPolicy + " </td></tr>" +
                        "<tr><td> Description </td><td> " + agreement.description + " </td></tr>" +
                        "<tr><td> Needs approval </td><td> " + agreement.needsApproval + " </td></tr>" +
                        "<tr><td> Is active </td><td> " + agreement.isActive + " </td></tr>" +
                        "<tr><td> Price </td><td> " + agreement.price + " </td></tr>" +
                        "<tr><td> Agent payment </td><td> " + agreement.PaymentAgent + " </td></tr>" +
                        "<tr><td> Commission value </td><td> " + agreement.commissionValue + " </td></tr>" +
                        "</tbody></table><br/><br>Click the following link to confirm ";


                    email.Email_to_send(agent.email, url1, content, subject);




                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Agreement/Create";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            
            return RedirectToAction("Index", "Error");
            }

            ViewBag.commissionType = new SelectList(db.commissionTypes, "ID", "label", agreement.commissionType);

            int partnerID = (int)Session["partnerID"];
            Partner part = db.Partners.Find(partnerID);

            if (part.isTest != true)
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest != true && a.isAgent == true), "ID", "tradeName", agreement.partnerID);
            }
            else
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest == true && a.isAgent == true), "ID", "tradeName", agreement.partnerID);
            }

            if (Session["admin"] != null)
            {
                ViewBag.serviceID = new SelectList(db.Services, "ID", "name");
            }
            else
            {
                ViewBag.serviceID = new SelectList(db.Services.Where(x => x.operatorID == partnerID), "ID", "name");
            }
            return View(agreement);
        }

        // GET: Agreements/Edit/5
        /// <summary>
        /// sets up the edit agreement view
        /// </summary>
        /// <param name="id">id of the agreement to be edited</param>
        /// <returns>the view of the form to edit the agreement</returns>
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
            Agreement agreement = db.Agreements.Find(id);
            if (agreement == null)
            {
                return HttpNotFound();
            }
            ViewBag.commissionType = new SelectList(db.commissionTypes, "ID", "label", agreement.commissionType);

            int partnerID = (int)Session["partnerID"];
            Partner part = db.Partners.Find(partnerID);

            if (part.isTest != true)
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest != true && a.isAgent == true), "ID", "tradeName", agreement.partnerID);
            }
            else
            {
                ViewBag.partnerID = new SelectList(db.Partners.Where(a => a.isTest == true && a.isAgent == true), "ID", "tradeName", agreement.partnerID);
            }



            if (Session["admin"] != null)
            {
                ViewBag.serviceID = new SelectList(db.Services, "ID", "name", agreement.serviceID);
            }
            else
            {
                ViewBag.serviceID = new SelectList(db.Services.Where(x => x.operatorID == partnerID), "ID", "name", agreement.serviceID);
            }


            int priceID = 3;
            int commissionID = 2;
            int paymentAgentID = 1;
            int availabilityID = 4;

            ViewBag.price = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == priceID);
            ViewBag.commission = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == commissionID);
            ViewBag.paymentAgent = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == paymentAgentID);
            ViewBag.availability = db.DynamicFieldAlternatives.Where(a => a.dynamicfieldID == availabilityID);


            return View(agreement);
        }

        // POST: Agreements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// post to the DB with the altered agreement 
        /// </summary>
        /// <param name="agreement">edited agreement to be posted</param>
        /// <returns>if edit successfull returns the agreement index view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,partnerID,serviceID,label,description,isActive,messageTemplate,agentInstructions,cancellationPolicy,needsApproval,price,commissionValue, priceType, TypeCommission, PaymentAgentType, PaymentAgent, Override1")] Agreement agreement)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var previous_agreement = db.Agreements.Where(x => x.ID == agreement.ID).FirstOrDefault();

            Logobj Changes = Log.Changes(previous_agreement, agreement);

            object ag = UpdateHelper(previous_agreement, agreement);
            ag = (Agreement)ag;

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(ag).State = EntityState.Modified;
                    db.SaveChanges();


                    AgreementLog agLog = new AgreementLog();
                    agLog.Date = System.DateTime.Now;
                    agLog.Time = System.DateTime.Now.ToString("HH:mm");
                    agLog.agreementID = previous_agreement.ID;
                    agLog.userID = (int)Session["userID"];
                    agLog.notes = Changes.changes;
                    db.AgreementLogs.Add(agLog);

                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "Agreement/Edit";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);
                
                    return RedirectToAction("Index", "Error");
                }
                return RedirectToAction("Index");
            }
            ViewBag.commissionType = new SelectList(db.commissionTypes, "ID", "label", agreement.commissionType);
            ViewBag.partnerID = new SelectList(db.Partners, "ID", "tradeName", agreement.partnerID);

            if (Session["admin"] != null)
            {
                ViewBag.serviceID = new SelectList(db.Services, "ID", "name");
            }
            else
            {
                int partnerID = (int)Session["partnerID"];
                ViewBag.serviceID = new SelectList(db.Services.Where(x => x.operatorID == partnerID), "ID", "name");
            }
            return View(agreement);
        }

        // GET: Agreements/Delete/5
        /// <summary>
        /// Option to delete an agreement has to pass a confirmation screen before actualy deleting the agreement
        /// Only available to admin. Normal users should set agreement to not active instead of deleting
        /// </summary>
        /// <param name="id">id of the agreement to be deleted</param>
        /// <returns>view with agreement details to confirm the admin is deleting the right agreement</returns>
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
            Agreement agreement = db.Agreements.Find(id);
            if (agreement == null)
            {
                return HttpNotFound();
            }
            return View(agreement);
        }

        // POST: Agreements/Delete/5
        /// <summary>
        /// post to delete the confirmed agreement from the DB
        /// </summary>
        /// <param name="id">id of the agreement to be deleted</param>
        /// <returns>the view to the agreement index</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {


            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            var agLog = db.AgreementLogs.Where(x => x.agreementID == id);

            foreach (AgreementLog a in agLog)
            {
                db.AgreementLogs.Remove(a);
            }

            var req = db.Requests.Where(x => x.agreementID == id);

            foreach (Request r in req)
            {
                db.Requests.Remove(r);
            }

            var reqLog = db.RequestLogs.Where(x => x.Request.agreementID == id);

            foreach (RequestLog rL in reqLog)
            {
                db.RequestLogs.Remove(rL);
            }

            try
            {
                Agreement agreement = db.Agreements.Find(id);
                db.Agreements.Remove(agreement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Agreements/Delete";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            
                return RedirectToAction("Index", "Error");
            }
        }

        /// <summary>
        /// redirect to the booking screen
        /// </summary>
        /// <param name="id">id of the agreement on which a request is to be performed</param>
        /// <returns>redirects the user to the booking view</returns>
        public ActionResult Book(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Session["AgreementID"] = id;
            return RedirectToAction("Index", "Booking");
        }

        /// <summary>
        /// confirms an agreement
        /// </summary>
        /// <param name="id">agreement id to be confirmed</param>
        /// <returns>the correspondig view to an confirmed agreement</returns>
        public ActionResult Confirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                Agreement ag = db.Agreements.Where(a => a.ID == id).First();
                ag.isConfirmed = true;
                db.Entry(ag).State = EntityState.Modified;
                db.SaveChanges();
                return View();
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Agreemtents/Confirmed";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            
                return RedirectToAction("Index", "Error");
            }
        }


        /// <summary>
        /// Displays the agreements in a list format without the associated images - only available to the admin 
        /// </summary>
        /// <returns>list view of the agreements</returns>
        public ActionResult List()
        {
            int? value = null;

            var agrList =
            from ag in db.Agreements
            join serv in db.Services on ag.serviceID equals serv.ID
            join part in db.Partners on ag.partnerID equals part.ID into partJoin
            from part in db.Partners.DefaultIfEmpty()
            join servTyp in db.serviceTypes on serv.typeID equals servTyp.ID
            join part2 in db.Partners on serv.operatorID equals part2.ID
            where ag.partnerID == part.ID || (value == null ? ag.partnerID == null : ag.partnerID == value) ///ag.partnerID.Equals(value)
            select new Agr_Serv_Partn_ServType { Service = serv, Partner = part, Agreement = ag, ServiceType = servTyp, Partner2 = part2 };

            return View(agrList.ToList());
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //The update of a DB object with new fields return from the view
        //Origanally created because framework was conflicting with model directly from view
        //
        public object UpdateHelper(object base_req, object view_req)
        {
            {
                if (base_req.GetType() != view_req.GetType())
                {
                    return null;
                }

                foreach (PropertyInfo propertyInfo in base_req.GetType().GetProperties())
                {
                    if (propertyInfo.CanRead)
                    {
                        object view_reqValue = propertyInfo.GetValue(view_req, null);
                        if (view_reqValue != null)
                        {
                            propertyInfo.SetValue(base_req, view_reqValue);
                        }
                    }
                }
                return base_req;
            }
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

            Req_User req_User = new Req_User
            {
                Description = ag.description,
                CommissionValue = ag.commissionValue,
                AgentInstructions = ag.agentInstructions,
                ConfirmationText = ag.messageTemplate,
                CancellationPolicy = ag.cancellationPolicy,
                PriceValue = ag.price,
                AgentPaymentValue = ag.PaymentAgent,
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
                req_User.Description = serv.description;
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

            return req_User;
        }




        [NonAction]
        public void FillGUIDS()
        {
            var users = db.Users;
            var partners = db.Partners;

            foreach(var us in users){
                us.UserIcalLink = Guid.NewGuid();
                us.passwordConfirm = us.password;
                db.Entry(us).State = EntityState.Modified;

            }

            foreach(var part in partners)
            {
                part.IcalLink = Guid.NewGuid();
                db.Entry(part).State = EntityState.Modified;

            }


            try
            {
                db.SaveChanges();
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
        }
    }
}
