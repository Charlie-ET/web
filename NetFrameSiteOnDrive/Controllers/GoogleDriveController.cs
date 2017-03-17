using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

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
                throw new System.Exception("s_drive is null");
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
        public ActionResult Safe()
        {
            return View();
        }

        public string Table()
        {
            return "table content";
        }
    }
}

