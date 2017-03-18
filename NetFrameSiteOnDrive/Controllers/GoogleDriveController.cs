using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

using NetFrameSiteOnDrive.Models;
using static NetFrameSiteOnDrive.StringCipher;

using log4net;

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

        private static readonly Lazy<ILog> s_lazyLogger = new Lazy<ILog>( 
            () =>{
                // Retrieve a logger for this context.
                ILog log = LogManager.GetLogger(typeof(GoogleDriveController));

                // Log some information to Google Stackdriver Logging.
                log.Info("GoogleDriveController logger created");
                return log;
            });
        private static ILog s_logger => s_lazyLogger.Value;

        [HttpGet]
        public async Task<ActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            s_logger.Debug("IndexAsync start");

            try
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

                        s_logger.Debug($"Credential user id {result.Credential.UserId}");
                        s_drive = new Drive(service);
                    }
                    else
                    {
                        s_logger.Debug($"Redirect {result.RedirectUri}");
                        return new RedirectResult(result.RedirectUri);
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error("auth failure", ex);
                throw;
            }

            if (s_drive == null)
            {
                s_logger.Error("s_drive is null", new Exception("s_drive is not created"));
                throw new Exception("s_drive is null");
            }

            //return Content(string.Join("<b/>", files.Select(x => x.Name)));

            // return Content(await s_drive.Download(files.First()));
            s_logger.Debug($"Return safe view");
            return View(nameof(Safe));
        }

        // GET: GoogleDrive/safe
        [HttpGet]
        public ActionResult Safe()
        {
            s_logger.Debug($"Entering safe");
            return View();
        }



        // Post: GoogleDrive/Table
        [HttpPost]
        public ActionResult SavePass(FileContent file)
        {
            s_logger.Debug($"Entering SavePass");
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            Debug.WriteLine(file.Content);

            // s_drive.MakeMimaCopy();
            s_drive.OverWriteMima(file.Content);

            return Content("good");
        }

        // Post: GoogleDrive/Table
        [HttpPost]
        public ActionResult EditPass(Credential pass)
        {
            s_logger.Debug($"Entering EditPass");
            if (pass == null)
            {
                throw new ArgumentNullException(nameof(pass));
            }
            Debug.WriteLine(pass.User);
            return Content("good");
        }

        private static bool s_switch = true;

        // GET: GoogleDrive/Table
        public async Task<ActionResult> TableAsync(CancellationToken cancellationToken)
        {
            s_logger.Debug($"Entering TableAsync");

            if (s_drive == null)
            {
                s_logger.Error($" TableAsync, s_drive is null");
                throw new Exception("s_drive?.MimaFile is null");
            }

            s_drive.MimaFile = await s_drive.GetLatest();
            if (s_drive.MimaFile == null)
            {
                s_logger.Error($" TableAsync, s_drive.MimaFile is null");
                return Content("CREATE_NEW");
            }

            s_logger.Debug($"download content");
            var content = await s_drive.Download(s_drive.MimaFile);
            //s_switch = !s_switch;
            s_switch = false;
            if (s_switch)
            {
                string pass = "pass111";
                return Content(Decrypt(content, pass));
            }
            else
            {
                s_logger.Debug($" TableAsync, return content ... {content.Substring(0, 5)}");
                return Content(content);
            }

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

