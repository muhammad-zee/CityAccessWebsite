using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Web.Helpers;

using System.Web.Mvc.Html;
using System.Web.UI.WebControls;
using System.Web.UI;
using CityAccess.Controllers;
using System.Globalization;

namespace CityAccess.Controllers
{
    public class PartnerSiteController : Controller
    {

        private CityAccessEntities db = new CityAccessEntities();

        // GET: PartnerSite
        public ActionResult Index()
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            return View();
        }


        //redirects to HeaderLinks tab View
        public ActionResult HeaderLinks()
        {

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }


            LinksPlus links = new CityAccess.LinksPlus();


            IQueryable<Link> queryable = null;
            var menuEntries = queryable;
            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            if (partnerSite != null)
            {
                menuEntries = db.Links.Where(a => a.partnerSiteID == partnerSite.ID).OrderBy(x => x.posHorizontal +" "+ x.posVertical).DefaultIfEmpty();
            }

            if(menuEntries.First() != null)
            {
                links.Links = menuEntries.ToList();
                var menusize = links.Links.Last().posHorizontal;
                ViewBag.MenuSize = menusize+1;
            }
            else
            {
                links.Links = new List<CityAccess.Link>();
                ViewBag.MenuSize = 0;
            }

            //links.Links.Add(new Link { value="abc", text ="abcd", posHorizontal=0, posVertical=0 });

            ViewBag.ListLength = links.Links.Count;

            for(int i = 0; i < links.Links.Count; i++)
            {
                links.Links[i].Index = i;
            }
            ViewBag.Tab = "abc";
            return View(links);
        }




        /// <summary>
        /// Saves PartnerSite menu 
        /// </summary>
        /// <param name="links">partner site menu links</param>
        /// <returns> Redirects to HeaderLinks view </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HeaderLinksPost(ICollection<Link> links)
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            if (links != null)
            {
                var partnerID = Session["partnerID"];
                var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

                if (partnerSite == null)     //Logo addition without having binary 
                {
                    partnerSite = new PartnerSite();

                    partnerSite.partnerID = (int)partnerID;

                    try
                    {
                        db.PartnerSites.Add(partnerSite);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        String url = "PartnerSite/HeaderLinks";
                        int userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);

                        return RedirectToAction("Index", "Error");
                    }
                }


                partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).First();

                foreach (var link in links)
                {
                    link.partnerSiteID = partnerSite.ID;

                    if (link.ID == 0)
                    {
                        db.Links.Add(link);
                    }
                    else
                    {
                        db.Entry(link).State = EntityState.Modified;
                    }
                }

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    String url = "PartnerSite/HeaderLinks";
                    int userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);

                    return RedirectToAction("Index", "Error");
                }
            }


            TempData["Tab"] = "links";
            return View("Index");
        }




        /// <summary>
        /// Delete a menu link 
        /// </summary>
        /// <param name="id">id of the link to be deleted</param>
        /// <returns> Redirects to HeaderLinks view </returns>
        [HttpPost]
        public Boolean DeleteLink(int? id)
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return false;
                }
            }


            try
            {
                Link link = db.Links.Find(id);

                if (link != null)
                {

                    if (link.IsDropMaster == true)
                    {
                        var links = db.Links.Where(x => x.posHorizontal == link.posHorizontal);

                        foreach (var lk in links)
                        {
                            db.Links.Remove(lk);
                        }
                    }
                    else
                    {
                        db.Links.Remove(link);
                    }
                    db.SaveChanges();
                }
            }

            catch (DbEntityValidationException dbEx)
            {

                String url = "PartnerSite/DeleteLink";
                int userID = (int)Session["userID"];
                auxMethods.ErrorHandling(dbEx, url, userID);

                return false;

            }
            return true;

        }


        //redirects to HomePageImg tab View
        public ActionResult HomePageImg()
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var bin = new CityAccess.BinaryPlus();
            var binar = new CityAccess.Binary();

            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            if (partnerSite != null)
            {
                binar = db.Binarys.Where(x => x.partnerSite_ID == partnerSite.ID && x.isBackground == true).FirstOrDefault();

                if (binar != null && binar.isBackground == true)
                {
                    bin.Binary1 = binar.Binary1;
                }
            }
            ViewBag.Tab = "abc";
            return View(bin);
        }

        //Saves partner site background image to the database

        //If no partnerSite created => creates new partnerSite and corresponding binary 
        //If no Binary exists => creates binary and associates it with corresponding partnerSite 
        //If Binary already exists => overrides the current binary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackgroundImg([Bind(Include="ID,Binary1,file,partnerSite_ID")]BinaryPlus binary)
        {

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }


            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            var bin = new CityAccess.Binary();

            if (binary.file != null)
            {
                bin.Binary1 = new WebImage(binary.file.InputStream).GetBytes();

                if (partnerSite == null)     //Logo addition without having binary 
                {
                    partnerSite = new PartnerSite();

                    partnerSite.partnerID = (int)partnerID;

                    db.PartnerSites.Add(partnerSite);
                    db.SaveChanges();

                    partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

                    bin.partnerSite_ID = partnerSite.ID;
                    bin.isBackground = true;


                    try
                    {
                        db.Binarys.Add(bin);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var url = "PartnerSite/BackgroundImg";
                        var userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);
                    }
                }
                else
                {

                    Binary binar = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == true).FirstOrDefault();

                    if (binar == null)
                    {
                        bin.partnerSite_ID = partnerSite.ID;
                        bin.isBackground = true;

                        try
                        {
                            db.Binarys.Add(bin);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "PartnerSite/BackgroundImg";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);
                        }
                    }
                    else
                    {
                        binar.Binary1 = bin.Binary1;

                        try
                        {
                            db.Entry(binar).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "PartnerSite/BackgroundImg";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);
                        }
                    }
                }
            }

            TempData["Tab"] = "homepage";

            return View("Index");
        }


        //redirects to Logo tab View
        public ActionResult Logo()
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var bin = new CityAccess.BinaryPlus();
            var binar = new CityAccess.Binary();

            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            if(partnerSite != null)
            {
                binar = db.Binarys.Where(x => x.partnerSite_ID == partnerSite.ID && x.isBackground == false).FirstOrDefault();

                if (binar != null && binar.isBackground == false)
                {
                    bin.Binary1 = binar.Binary1;
                }
            }

            ViewBag.Tab = "abc";
            return View(bin);
        }




        /// <summary>
        /// Saves partner logo to be displayed in partner site
        /// </summary>
        /// <param name="binary">The binary for logo file to be saved</param>
        /// <returns>Redirects to Logo View</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogoPost([Bind(Include = "ID, Binary1, file, LogoPos")]BinaryPlus binary)
        {

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }


            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            var bin = new CityAccess.Binary();

            if (binary.file != null)
            {
                bin.Binary1 = new WebImage(binary.file.InputStream).GetBytes();

                if (partnerSite == null)     //Logo addition without having binary 
                {
                    partnerSite = new PartnerSite();

                    partnerSite.partnerID = (int)partnerID;

                    try
                    {
                        db.PartnerSites.Add(partnerSite);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var url = "PartnerSite/LogoPost";
                        var userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);
                    }

                    partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

                    bin.partnerSite_ID = partnerSite.ID;
                    bin.isBackground = false;

                    try
                    {
                        db.Binarys.Add(bin);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        var url = "PartnerSite/LogoPost";
                        var userID = (int)Session["userID"];

                        auxMethods.ErrorHandling(dbEx, url, userID);
                    }

                }
                else
                {

                    Binary binar = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == false).FirstOrDefault();

                    if (binar == null)
                    {
                        bin.partnerSite_ID = partnerSite.ID;
                        bin.isBackground = false;

                        try
                        {
                            db.Binarys.Add(bin);
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "PartnerSite/LogoPost";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);
                        }
                    }
                    else
                    {
                        binar.Binary1 = bin.Binary1;

                        try
                        {
                            db.Entry(binar).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            var url = "PartnerSite/LogoPost";
                            var userID = (int)Session["userID"];

                            auxMethods.ErrorHandling(dbEx, url, userID);
                        }
                    }
                }
            }

            TempData["Tab"] = "logo";
            return RedirectToAction("Index");
        }




        //redirects to Style tab View
        public ActionResult Style()
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            Style SiteStyle = db.Styles.Where(x => x.partnerSiteID == partnerSite.ID).FirstOrDefault();

            if (SiteStyle != null)
            {
                SiteStyle.Font = partnerSite.FontStyle;
            }

            ViewBag.Fonts = db.FontStyles;

            return View(SiteStyle);
        }





        /// <summary>
        /// Saves style preferences to the database
        /// </summary>
        /// <param name="fontID"> font of partner site menu </param>
        /// <param name="style"> style options for partner site</param>
        /// <returns>Redirects to Style tab view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StylePost(String fontID, Style style)
        {

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            if (partnerSite == null)     //Logo addition without having binary 
            {
                partnerSite = new PartnerSite();

                partnerSite.partnerID = (int)partnerID;


                try
                {
                    db.PartnerSites.Add(partnerSite);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    var url = "PartnerSite/StylePost";
                    var userID = (int)Session["userID"];

                    auxMethods.ErrorHandling(dbEx, url, userID);
                }


                partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();
            }

            var BinBackgroundImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == true).FirstOrDefault();

            var BinLogoImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == false).FirstOrDefault();

            var StyleExists = true;

            //Storing information in styles table
            Style SiteStyle = db.Styles.Where(x => x.partnerSiteID == partnerSite.ID).FirstOrDefault();

            if (SiteStyle == null)
            {
                StyleExists = false;
                style.partnerSiteID = partnerSite.ID;
            }
            else
            {
                SiteStyle.BackgroundColor = style.BackgroundColor;
                SiteStyle.FontColor = style.FontColor;
                SiteStyle.MenuPosition = style.MenuPosition;
                SiteStyle.LogoPosition = style.LogoPosition;

            }

            //Image sizes to not maintain original quality
            var path = Server.MapPath("/Content/NoImage.png");
            var filestream = new FileStream(path, FileMode.Open);

            WebImage bckg = new WebImage(filestream);

            if (BinBackgroundImg != null)
            {
                 bckg = new WebImage(BinBackgroundImg.Binary1);
            }

            
            var bckgRatio = (float)bckg.Height / (float)bckg.Width;

            WebImage logo = bckg;
            if (BinLogoImg.Binary1 != null)
            {
              logo = new WebImage(BinLogoImg.Binary1);
            }
            var logoRatio = (float)logo.Width / (float)logo.Height;

            var logoWidth = 50 * logoRatio;

            var fixedCss = ".PartnerSiteHeader { width: 100%; padding-left:3%;padding-right: 5%; padding-bottom: 7%;padding-top: 4%; height: 10vh; background-color:"+style.BackgroundColor+"}"
                + ".PartnerSiteBody{width:100%;min-height: 90vh;position:relative;height:max-content;} .PartnerSiteBody .BackgroundImg{width:100vw;height:" + (int)(bckgRatio*100)+"vw;max-height:78vh;z-index:1;position:absolute;margin-top:14vh;}" +
                ".Navbar {overflow: hidden;}" +
                ".Navbar a { float: left;font-size: 14px; color: "+style.FontColor+";text-align: center; padding: 14px 16px;text-decoration: none;}" +
                ".dropdown {float: left;overflow: hidden;}" +
                ".dropdown .dropbtn {font-size: 14px; border: none;outline: none;color: rgb(110, 110, 160); padding: 14px 16px;background-color: inherit; font-family: inherit;margin: 0;} " +
                ".navbar a:hover,.dropdown:hover .dropbtn{color: rgb(45, 45, 45); }" +
                ".dropdown-content{display: none; position: relative;background-color: #f9f9f9;min-width: 160px;box-shadow: 0px 8px 16px 0px rgba(0,0,0,0.2); z-index: 30;}" +
                ".dropdown-content a{float:none; color:black; padding: 12px 16px; text-decoration:none; display:block;}" +
                ".dropdown-content a:hover{background-color: #ddd !important;}" +
                ".dropdown a{color: #484c61; font-weight: 550;}" +
                ".dropdown:hover .dropdown-content{display: block !important;}" +
                "footer {background-color: " + style.BackgroundColor + ";height: 2.5rem;border-top: 1px solid #111;bottom: 0;min-width:calc(100vw - 200px); padding: 0.5 % 1 %;position:absolute;z-index:1000;}" +
                ".PartnerSiteLogoPos img {height:50px; width:"+logoWidth+"px;}";

            var fontStyle = ".PartnerSiteFont{ font-family:"+fontID+";color:"+style.FontColor+";}";

            var MenuPosVal = string.Empty;

            if(style.MenuPosition == true)
            {
                MenuPosVal = "Right";
            }
            else
            {
                MenuPosVal = "Left";
            }

            var MenuPosCss = ".PartnerSiteMenuPos{ float:"+MenuPosVal+";background-color:"+ style.BackgroundColor +"} .mobileMenu{ float:"+MenuPosVal+"}";

            if ( MenuPosVal == "Left")
            {
                MenuPosCss = MenuPosCss + ".mobileMenu { right: 60vw;} @media (max-width: 768px) {.dropdown-content{ left: 20vw; text-align:left;}}";
            }
            else
            {
                MenuPosCss = MenuPosCss + ".mobileMenu { left: 60vw;} @media (max-width: 768px) {.dropdown-content{ right: 20vw; text-align:right;}}";
            }

            var LogoPosVal = string.Empty;

            if (style.LogoPosition == true)
            {
                LogoPosVal = "Right";
            }
            else
            {
                LogoPosVal = "Left";
            }

            var LogoPosCss = ".PartnerSiteLogoPos{ float:" + LogoPosVal + ";}";


            var BackgroundColor = ".PartnerSiteBackgroundColor{background-color:" + style.BackgroundColor + ";position:relative;height: 100vh;}";

            var css = fontStyle + MenuPosCss + BackgroundColor + LogoPosCss + fixedCss;


            partnerSite.css = css;
            if (style.Font != null)
            {
                partnerSite.FontStyle = style.Font;
            }
            //Add try catch with error 
            db.Entry(partnerSite).State = EntityState.Modified;

            if (StyleExists)
            {
                db.Entry(SiteStyle).State = EntityState.Modified;
            }
            else
            {
                db.Styles.Add(style);
            }


            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "PartnerSite/StylePost";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            }


            ViewBag.Fonts = db.FontStyles;

            TempData["Tab"] = "style";
            return RedirectToAction("Index");
        }




        //redirects to Advanced tab View
        public ActionResult Advanced()
        {
            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var partnerID = Session["partnerID"];

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            return View(partnerSite);
        }



        /// <summary>
        /// Saves custom changes to partner site css
        /// </summary>
        /// <param name="partSite">PartnerSite class with css modified string to be saved</param>
        /// <returns>Redirects to Advanced tab view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdvancedPost(PartnerSite partSite)
        {

            if (Session["admin"] == null)
            {
                if (Session["userID"] == null)
                {
                    return RedirectToAction("Index", "Login");
                }
            }

            var partnerSite = db.PartnerSites.Where(x => x.ID == partSite.ID).FirstOrDefault();

            partnerSite.css = partSite.css;

            try
            {
                db.Entry(partnerSite).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                var url = "PartnerSite/AdvancedPost";
                var userID = (int)Session["userID"];

                auxMethods.ErrorHandling(dbEx, url, userID);
            }

            TempData["Tab"] = "advanced";
            return RedirectToAction("Index");
        }



 /////////////// From this point below the functions are related to the client pages //////////////////////////////////////////////////////////////////////////////




        /// <summary>
        /// Fetchs the database for necessary info for presenting PartnerBookingPage
        /// </summary>
        /// <param name="agID">ID of agreement to present</param>
        /// <param name="partnerID">Id of the partner associated to the partner site to present</param>
        /// <returns>Partner Booking Page view</returns>
        public ActionResult PartnerBookingPage(int? agID, int? partnerID )
        {

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == partnerID).FirstOrDefault();

            var BinBackgroundImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == true).FirstOrDefault();

            var BinLogoImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == false).FirstOrDefault();

            PartnerSiteElements siteEL = new PartnerSiteElements();

            siteEL.BackgroundImage = BinBackgroundImg.Binary1;

            siteEL.Logo = BinLogoImg.Binary1;

            var links = db.Links.Where(a => a.partnerSiteID == partnerSite.ID).OrderBy(x => x.posHorizontal + " " + x.posVertical).DefaultIfEmpty();

            siteEL.Links = links.ToList();

            siteEL.PartnerID = (int)partnerID;

            TempData["ag"] = agID;

            return View(siteEL);
        }





        /// <summary>
        /// Fetchs the database for necessary info for presenting PartnerAgreementPage
        /// </summary>
        /// <param name="partnerID">Id of the partner associated to the partner site to present</param>
        /// <returns>Partner Agreement Page view</returns>
        public ActionResult PartnerAgreementPage(int? partnerID)
        {

            var partnerSite = db.PartnerSites.Where(x => x.partnerID == partnerID).FirstOrDefault();

            var BinBackgroundImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == true).FirstOrDefault();

            var BinLogoImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == false).FirstOrDefault();

            PartnerSiteElements siteEL = new PartnerSiteElements();

            siteEL.BackgroundImage = BinBackgroundImg.Binary1;

            siteEL.Logo = BinLogoImg.Binary1;

            var links = db.Links.Where(a => a.partnerSiteID == partnerSite.ID).OrderBy(x => x.posHorizontal + " " + x.posVertical).DefaultIfEmpty();

            siteEL.Links = links.ToList();

            siteEL.PartnerID = (int)partnerID;

            return View(siteEL);
        }





        /// <summary>
        /// Fetchs the database for necessary info for presenting BookingSuccessfull view
        /// </summary>
        /// <param name="partnerID">Id of the partner associated to the partner site to present</param>
        /// <returns>Partner BookingSuccessfull view</returns>
        public ActionResult BookingSuccessfull(int? partnerID)
        {
            var partnerSite = db.PartnerSites.Where(x => x.partnerID == partnerID).FirstOrDefault();

            var BinBackgroundImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == true).FirstOrDefault();

            var BinLogoImg = db.Binarys.Where(a => a.partnerSite_ID == partnerSite.ID && a.isBackground == false).FirstOrDefault();

            PartnerSiteElements siteEL = new PartnerSiteElements();

            siteEL.BackgroundImage = BinBackgroundImg.Binary1;

            siteEL.Logo = BinLogoImg.Binary1;

            var links = db.Links.Where(a => a.partnerSiteID == partnerSite.ID).OrderBy(x => x.posHorizontal + " " + x.posVertical).DefaultIfEmpty();

            siteEL.Links = links.ToList();

            siteEL.PartnerID = (int)partnerID;

            return View(siteEL);
        }




        /// <summary>
        /// Function to fetch css for partner site and include it in view
        /// </summary>
        /// <param name="partnerID">Id of partner to fetch css</param>
        /// <returns>partner site css file</returns>
        public ActionResult GetCss(int partnerID)
        {
            var partnerSite = db.PartnerSites.Where(x => x.partnerID == (int)partnerID).FirstOrDefault();

            return Content(partnerSite.css, "text/css");
        }
    }
}