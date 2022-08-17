using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CityAccess.Controllers
{
    public class MarketController : Controller
    {

        private CityAccessEntities db = new CityAccessEntities();

        // GET: Market
        public ActionResult Index(string searchFilter)
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

                var partners = db.Partners.Where(x => x.isOperator == true && x.isPublic == true && x.isTest != true);

                //if (!String.IsNullOrEmpty(TradeName))
                //{
                //    partners = partners.Where(s => s.tradeName == TradeName);
                //}

                if (!String.IsNullOrEmpty(searchFilter))
                {
                    partners = partners.Where(x => x.description.Contains(searchFilter) || x.tradeName.Contains(searchFilter));
                }


            return View(partners);
        }

        // GET: Market/Details/5
        public ActionResult Details(int id, string city)
        {
            if (Session["admin"] == null && Session["userID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var services = db.Services.Where(x => x.operatorID == id && x.isPublic == true);

            ViewBag.cities = (from serv in db.Services
                              join cities in db.Cities on serv.cityID equals cities.ID
                              where serv.operatorID == id
                              select cities).Distinct();

            Partner part = db.Partners.Where(x=> x.ID == id).First();
            ViewBag.operator1 = part.tradeName;

            if (!String.IsNullOrEmpty(city))
            {
               services = services.Where(x => x.City.name == city);
            }
            return View(services);
        }



    }
}