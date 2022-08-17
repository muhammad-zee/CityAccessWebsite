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
    public class RequestLogController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: RequestLog
        public ActionResult Index()
        {
            var reqLog = db.RequestLogs.Include(s => s.User);
            return View(reqLog.ToList());
        }

        // GET: ServicesBooked/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestLog reqLog = db.RequestLogs.Find(id);

            if (reqLog == null)
            {
                return HttpNotFound();
            }
            return View(reqLog);
        }
    }
}