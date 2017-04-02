using System;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

using System.IO;
using log4net;


namespace NetFrameSiteOnDrive
{
    public class AppFlowMetadata : FlowMetadata
    {

        private static ILog s_logger => s_lazyLogger.Value;
        private static readonly Lazy<ILog> s_lazyLogger = new Lazy<ILog>(
            () => {
                // Retrieve a logger for this context.
                ILog log = LogManager.GetLogger(typeof(AppFlowMetadata));

                // Log some information to Google Stackdriver Logging.
                log.Info("AppFlowMetadata logger created");
                return log;
            });

        public static readonly Lazy<string> s_clientId = new Lazy<string>(() => OpenFileGetLine("~/clientid.txt"));
        private static readonly Lazy<string> s_lazySecret = new Lazy<string>(() => OpenFileGetLine("~/secret.txt"));

        private static string OpenFileGetLine(string relativePath)
        {
            string localFile = System.Web.Hosting.HostingEnvironment.MapPath(relativePath);
            s_logger.Debug($"open {localFile}");
            using (var sr = new StreamReader(localFile))
            {
                var sec = sr.ReadLine().Trim();
                if (string.IsNullOrEmpty(sec))
                {
                    s_logger.Error($"empty {localFile}");
                    throw new Exception("Failed to get secret");
                }

                return sec;
            }
        }

        public static Lazy<string> s_authDataStoreFile = new Lazy<string>(() => OpenFileGetLine("~/authStoreLocation.txt"));

        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = s_clientId.Value,
                    ClientSecret = s_lazySecret.Value
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new FileDataStore(s_authDataStoreFile.Value)
            });

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }
            return user.ToString();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get {
                var gflow = flow as GoogleAuthorizationCodeFlow;
                s_logger.Debug($"secret is {gflow.ClientSecrets.ClientSecret.Substring(2, 5)}");
                return flow;
            }
        }
    }
}