using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SiteOnGoogleDrive.Controllers
{
    public class GoogleDriveController : Controller
    {
        //public IActionResult Index()
        public string Index()
        {
            // return View();
            // return "This is my <b>default</b> action";
            return String.Join("<b></b>", (new GoogleDriveLib.FileExplorer()).ListDriveFiles(10));
        }

        public string Password()
        {
            return "This is the password page.";
        }
    }
}