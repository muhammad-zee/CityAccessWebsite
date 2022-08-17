using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CityAccess.Controllers
{
    public class ErrorController : Controller
    {

        private CityAccessEntities db = new CityAccessEntities();
        // GET: Error
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        [NonAction]
        public void ErrorLog(int user, string error, string url)
        {
            ErrorLog errorToEnter = new ErrorLog();

            DateTime? date = System.DateTime.Now;

            DateTime midnigth = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0);

            TimeSpan? time = date - midnigth;

            errorToEnter.User = user;
            errorToEnter.errorDate = date;
            errorToEnter.errorTime = time;
            errorToEnter.errorURL = url;
            errorToEnter.errorMsg = error;

            try
            {
                db.ErrorLogs.Add(errorToEnter);
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