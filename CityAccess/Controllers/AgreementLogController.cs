using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace CityAccess.Controllers
    {
        public class AgreementLogController : Controller
        {
            private CityAccessEntities db = new CityAccessEntities();

            // GET: AgreementLog
            public ActionResult Index()
            {
                var agLog = db.AgreementLogs.Include(s => s.User);
                return View(agLog.ToList());
            }

            // GET: ServicesBooked/Details/5
            public ActionResult Details(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                AgreementLog agLog = db.AgreementLogs.Find(id);

                if (agLog == null)
                {
                    return HttpNotFound();
                }
                return View(agLog);
            }
        }
    }

