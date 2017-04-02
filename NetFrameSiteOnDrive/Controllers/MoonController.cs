using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetFrameSiteOnDrive.Controllers
{
    public class MoonController : Controller
    {
        // GET: Moon
        public ActionResult Index()
        {
             
            return View();
        }

        // GET: Moon/Table
        public ActionResult Table()
        {
            return View();
        }

        // GET: Moon/Angular
        public ActionResult Angular()
        {
            return View();
        }

        // Get: Moon/Password
        public ActionResult Password()
        {
            return View();
        }
    }
}