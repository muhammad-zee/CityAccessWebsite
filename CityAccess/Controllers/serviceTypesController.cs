using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CityAccess
{
    public class serviceTypesController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: serviceTypes
        public ActionResult Index()
        {
            return View(db.serviceTypes.ToList());
        }

        // GET: serviceTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            serviceType serviceType = db.serviceTypes.Find(id);
            if (serviceType == null)
            {
                return HttpNotFound();
            }
            return View(serviceType);
        }

        // GET: serviceTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: serviceTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,name,description")] serviceType serviceType)
        {
            if (ModelState.IsValid)
            {
                db.serviceTypes.Add(serviceType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(serviceType);
        }

        // GET: serviceTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            serviceType serviceType = db.serviceTypes.Find(id);
            if (serviceType == null)
            {
                return HttpNotFound();
            }
            return View(serviceType);
        }

        // POST: serviceTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,name,description")] serviceType serviceType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(serviceType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(serviceType);
        }

        // GET: serviceTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            serviceType serviceType = db.serviceTypes.Find(id);
            if (serviceType == null)
            {
                return HttpNotFound();
            }
            return View(serviceType);
        }

        // POST: serviceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            serviceType serviceType = db.serviceTypes.Find(id);
            db.serviceTypes.Remove(serviceType);
            db.SaveChanges();
            return RedirectToAction("Index");
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
