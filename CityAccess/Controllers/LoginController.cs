using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity.Validation;

namespace CityAccess.Controllers
{
    public class LoginController : Controller
    {
        private CityAccessEntities db = new CityAccessEntities();

        // GET: Login
        public ActionResult Index()
        {
            
            return View();
        }

        // GET: Login/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index","Home");
        }
        // Post: Login/Authorize
        [HttpPost]
        public ActionResult Authorize(CityAccess.User userModel)
        {
            userModel.password = Encryptor.MD5Hash(userModel.password);
            var userInfo = db.Users.Where(x => x.username == userModel.username && x.password == userModel.password).FirstOrDefault();
            if(userInfo == null)
            {
                userModel.LoginErrorMessage = "Wrong username or password.";
                //ModelState["password"].Value = new ValueProviderResult(string.Empty, string.Empty, ModelState["password"].Value.Culture);
                ModelState.Clear();
                return View("Index", userModel);
            }
            if(userInfo.emailConfirmed != true)
            {
                userModel.LoginErrorMessage = "You have to validate your account via e-mail.";
                return View("Index", userModel);
            }
            else
            {              
                if (userInfo.username == "admin_CA")
                {
                    Session["admin"] = userInfo.username;
                }
                else
                {
                    Partner partner = db.Partners.Find(userInfo.partnerId);
                    Session["userID"] = userInfo.ID;
                    Session["username"] = userInfo.username;
                    Session["partner"] = partner.tradeName;                  
                    Session["partnerID"] = userInfo.partnerId;

                    if (userInfo.isAdmin == true)
                    {
                        Session["userAdmin"] = true;
                    }

                    if(partner.isOperator == true)
                    {
                        Session["operator"] = true;
                    }
                    if (partner.isAgent == true)
                    {
                        Session["agent"] = true;
                    }


                }
                Session["userID"] = userInfo.ID;


                userInfo.lastLoginDate = System.DateTime.Now;
                userInfo.passwordConfirm = userInfo.password;

                try
                {
                    db.Entry(userInfo).State = EntityState.Modified;
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

                return RedirectToAction("Index", "Agreements");
            }
        }


        [NonAction]
        public Boolean Login_Check()
        {
            if (Session == null)
            {
                    return true;

            }


            return false;
        }
    }
}