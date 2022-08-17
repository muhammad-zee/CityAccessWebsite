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
    public class commissionTypesController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: commissionTypes
        public ActionResult Index()
        {
            return View(db.commissionTypes.ToList());
        }

        // GET: commissionTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            commissionType commissionType = db.commissionTypes.Find(id);
            if (commissionType == null)
            {
                return HttpNotFound();
            }
            return View(commissionType);
        }

        // GET: commissionTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: commissionTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,label,description")] commissionType commissionType)
        {
            if (ModelState.IsValid)
            {
                db.commissionTypes.Add(commissionType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(commissionType);
        }

        // GET: commissionTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            commissionType commissionType = db.commissionTypes.Find(id);
            if (commissionType == null)
            {
                return HttpNotFound();
            }
            return View(commissionType);
        }

        // POST: commissionTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,label,description")] commissionType commissionType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(commissionType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(commissionType);
        }

        // GET: commissionTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            commissionType commissionType = db.commissionTypes.Find(id);
            if (commissionType == null)
            {
                return HttpNotFound();
            }
            return View(commissionType);
        }

        // POST: commissionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            commissionType commissionType = db.commissionTypes.Find(id);
            db.commissionTypes.Remove(commissionType);
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
