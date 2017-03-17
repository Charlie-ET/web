using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

using static NetFrameSiteOnDrive.StringCipher;

namespace NetFrameSiteOnDrive.Controllers
{
    public class GoogleDriveController : AsyncController
    {
        //public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        //{
        //    return Content("Async controller");
        //}

        //public ActionResult Index()
        //{
        //    return Content("something");
        //}

        private static Drive s_drive = null;


        [HttpGet]
        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            if (s_drive == null)
            {
                var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).
                    AuthorizeAsync(cancellationToken);

                if (result.Credential != null)
                {
                    var service = new DriveService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = result.Credential,
                        ApplicationName = "Site on Google Drive"
                    });

                    s_drive = new Drive(service);
                }
                else
                {
                    return new RedirectResult(result.RedirectUri);
                }
            }

            if (s_drive == null)
            {
                throw new Exception("s_drive is null");
            }

            var files = await s_drive.EnumerateFiles(rootFolder: "mima");
            if (files?.Count() == 0)
            {
                return Content("No file is found.");
            }
            //return Content(string.Join("<b/>", files.Select(x => x.Name)));

            // return Content(await s_drive.Download(files.First()));
            return View(nameof(Safe));
        }

        // GET: GoogleDrive/safe
        [HttpGet]
        public ActionResult Safe()
        {
            return View();
        }



        // Post: GoogleDrive/Table
        [HttpPost]
        public async Task<ActionResult> SavePassAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException(nameof(SavePassAsync));
        }

        // GET: GoogleDrive/Table
        public async Task<ActionResult> TableAsync(CancellationToken cancellationToken)
        {
            if (s_drive == null)
            {
                throw new Exception("s_drive is null");
            }

            var files = await s_drive.EnumerateFiles(rootFolder: "mima");
            if (files?.Count() == 0)
            {
                return Content("No file is found.");
            }

            var content = await s_drive.Download(files.First());
            string pass = "pass111";
            return Content(Decrypt(content, pass));
        }

        // GET: GoogleDrive/TableTest
        public string TableTest()
        {
            // return "table content";
            return @"
{
""records"":[
    { ""id"":""gmail"", 
      ""user"":""deren.last@gmail.com"", 
      ""pass"":""you can guess"", 
      ""desc"":""my personal gmail account"" },
    { ""id"":""keyBank"", 
      ""user"":""deren.last@gmail.com"", 
      ""pass"":""give me all your money"",
      ""desc"":""Key Bank access"" }
]}";
        }
    }
}

