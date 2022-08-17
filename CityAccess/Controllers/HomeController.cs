using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CityAccess.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return View();
            return View("~/Landing/landing.cshtml");
        }

        [HttpPost]
        public String Index(string name, string email, string subject, string message)
        {
            EmailController email1 = new EmailController();

            message = BreakLN.ProcessBrkHTML(message);
            message = "Name: "+ name +" e-mail: "+ email +"<br/><br>" + message;
            email1.Email_to_send("info@cityaccess.pt", null, message, subject);

            //Ok is what the ajax post expects to show success
            return "OK";
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
      
            return View();
        }

        //post para o contact , mandar e-mail
        [HttpPost]
        public ActionResult Contact( string content)
        {
            //create e-mail 
            EmailController email = new EmailController();
            string subject = string.Empty;

            //if login send partner and user in title 
            if(Session["userID"] != null && Session["admin"] == null)
            {
               subject = "Feedback from " +(string)Session["username"] + " - " + (string)Session["partner"];
            }
            else
            {
                subject = "Feedback from anonymous user.";
            }

            content = BreakLN.ProcessBrkHTML(content);
            email.Email_to_send("info@cityaccess.pt", null, content, subject);

            return View("Feedback");
        }
    }
}