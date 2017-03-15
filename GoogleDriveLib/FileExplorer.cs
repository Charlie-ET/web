using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDriveLib
{
    public class FileExplorer
    {
        //private void ReadCredentialFile(string filePath = @"c:\secret\pacific-6b0cd9914e75.p12")
        //{
        //    string[] scopes = new string[] { DriveService.Scope.Drive }; // Full access

        //    //var keyFilePath = @"c:\file.p12";    // Downloaded from https://console.developers.google.com
        //    var serviceAccountEmail = "xx@developer.gserviceaccount.com";  // found https://console.developers.google.com

        //    //loading the Key file
        //    var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
        //    var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
        //    {
        //        Scopes = scopes
        //    }.FromCertificate(certificate));
        //}

        public IEnumerable<string> ListDriveFiles(int maxCount)
        {
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                @"c:\secret\hackathon-site-on-drive-test.json");

            var task = GoogleCredential.GetApplicationDefaultAsync();
            task.Wait();
            GoogleCredential credential = task.Result;

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "What Name"
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                return files.Select(x => x.Name).Take(maxCount);
            }

            return null;
        }
    }
}
