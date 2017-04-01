using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Template_AspNetMvc.Controllers
{
    public class PeanutController : Controller
    {
        // GET: Peanut
        public ActionResult Index()
        {
            Session["Token"] = Guid.NewGuid().ToString();
            return View();
        }

        // GET: /Peanut/Log/1
        [HttpGet]
        public ActionResult LogDefault()
        {
            return Content("Yes, Log()");
        }

        // GET: /Peanut/Log/1
        [HttpGet]
        public ActionResult Log(int? count)
        {
            return Content($"{count.ToString()}  {Session["Token"]}");
        }
    }
}