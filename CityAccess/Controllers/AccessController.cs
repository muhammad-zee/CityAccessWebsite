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

namespace CityAccess.Controllers
{
    public class AccessController : Controller
    {
        // GET: Access
        public void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
        }
        // Returns true if user has access to user information
        public Boolean UserAccess(int? id)
        {
            if (id != (int)Session["userID"] && Session["admin"] == null)
            {
                return false;
            }

            return true;
        }

        //Returns true if user has access to partner information
        public Boolean PartnerAccess(int? id)
        {
            if (Session["admin"] == null)
            {
                if (id != (int)Session["partnerID"])
                {
                    return false;
                }
            }
            return true;
        }
    }
}