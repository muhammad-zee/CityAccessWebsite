using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace CityAccess.Controllers
{
    

    public class RegisterController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Register
        public ActionResult Index(int? inv)
        {
            if(inv != null)
            {
                ViewBag.frominv = 1;
                ViewBag.partID = inv;
            }
            else //Because the only method to register now is through invite 
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            User us = new User();
            ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName");
            return View(us);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,username,fullName,password,email,phone,lastLoginDate,partnerId,isActive, emailConfirmed, passwordConfirm")] User userModel)
        {
            db.Configuration.ValidateOnSaveEnabled = false;

            ViewBag.partnerId = new SelectList(db.Partners, "ID", "tradeName");
            var userInfo = db.Users.Where(x => x.username == userModel.username).FirstOrDefault();

            //Defining if the user is admin or not
            User us1 = db.Users.Where(z => z.partnerId == userModel.partnerId).FirstOrDefault();
            if(us1 == null)
            {
                userModel.isAdmin = true;
            }

            //fetching do email do user admin
            string useradmin = string.Empty;
            if (us1 != null)
            {
                User us = db.Users.Where(z => z.isAdmin == true && z.partnerId == userModel.partnerId).First();
                if (us.emailConfirmed == true)
                {
                    useradmin = us.email;
                }
            }

            if (userInfo != null)
            {
                userModel.RegisterErrorMessage = "Username already exists.";
                if(userModel.emailConfirmed == true)
                {
                    ViewBag.frominv = 1;
                    ViewBag.partID =userModel.ID;
                }
                return View("Index", userModel);
            }

            userInfo = db.Users.Where(x => x.email == userModel.email).FirstOrDefault();

            if (userInfo != null)
            {
                userModel.RegisterErrorMessage = "The e-mail is already linked to an account.";
                if (userModel.emailConfirmed == true)
                {
                    ViewBag.frominv = 1;
                    ViewBag.partID = userModel.ID;
                }
                return View("Index", userModel);
            }
            else
            {
                try
                {
                    //if (ModelState.IsValid)
                    //{
                        
                        UserLog usLog = new UserLog();
                        usLog.Date = System.DateTime.Now;
                        usLog.Time = System.DateTime.Now.ToString("HH:mm");
                        usLog.userID = userModel.ID;
                        usLog.editorID = (int)Session["userID"];
                        usLog.notes ="User created!";
                        db.UserLogs.Add(usLog);


                        userModel.password = Encryptor.MD5Hash(userModel.password);

                        db.Users.Add(userModel);
                        db.SaveChanges();

                    string msg = string.Empty;
                    if (userModel.emailConfirmed != true)
                    {
                        SendConfirmationEmail(useradmin, userModel.ID, userModel.username);
                        msg = "Registration successfull. Account activation link, has been sent to your email:"
                           + userModel.email;
                    }
                    else
                    {
                        msg = "Your user account was successfully created, you can log in into City Access";
                    }
                    ViewBag.Message = msg;
                        return RedirectToAction("Index", "Login");
                        //return RedirectToAction("Index", "Users");
                    //}
                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "Register/Create";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);

                    return RedirectToAction("Index", "Error");
                }
            }
        }

        public ActionResult VerifyAccount(int id)
        {

            db.Configuration.ValidateOnSaveEnabled = false;
            User user = db.Users.Find(id);

            try
            {
                user.emailConfirmed = true;
                Session["userID"] = user.ID;
                Session["partnerID"] = user.partnerId;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                
                return View();
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "Register/VerifyAccount";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);

                return RedirectToAction("Index", "Error");
            }
        }

        [NonAction]
        public void SendConfirmationEmail(string emailID, int userID, string username)
        {
            var verifyUrl = "/Register/VerifyAccount?ID=" + userID;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);


            string success = "Registration successfull.";
            string content = "<br/><br> Your CityAccess account for "+ username +" was successfuly created."+
                "Follow the link below to complete your registration. ";

            EmailController email = new EmailController();
            // email.Email_to_send(emailID, link, content, success);

            // For the time being only the administrators can approve the e-mail
            email.Email_to_send("CityAccess@cityaccess.pt", link, content, success);
            if (emailID != string.Empty)
            {
                email.Email_to_send(emailID, link, content, success);
            }
        }
    }
}