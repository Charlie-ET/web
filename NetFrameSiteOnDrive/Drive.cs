using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using DriveData = Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Download;
using System.IO;

using log4net;

namespace NetFrameSiteOnDrive
{
    public class Drive
    {
        private static ILog s_logger => s_lazyLogger.Value;
        private static readonly Lazy<ILog> s_lazyLogger = new Lazy<ILog>(
            () => {
                // Retrieve a logger for this context.
                ILog log = LogManager.GetLogger(typeof(Drive));

                // Log some information to Google Stackdriver Logging.
                log.Info("Drive logger created");
                return log;
            });

        private const uint MaxListFileCount = 100;
        private const int ListFileBatchFileCount = 100;
        private DriveService _service;

        public DriveData.File MimaFile { get; set; }
        private DriveData.File _mimaFolder;

        public Drive(DriveService service)
        {         
            if (service == null)
            {
                s_logger.Error("service input is null", new ArgumentNullException(nameof(service)));
                throw new ArgumentNullException(nameof(service));
            }
            s_logger.Debug("construct Drive");
            _service = service;
        }

        //public void MakeMimaCopy()
        //{
        //    if (MimaFile == null)
        //    {
        //        return;
        //    }
        //    DriveData.File copiedFile = new DriveData.File();
        //    var copyName = DateTime.Now.ToString(".yyyy-dd-M--HH-mm-ss-fff");
        //    copiedFile.Name = MimaFile.Name + copyName;
        //    _service.Files.Copy(copiedFile, MimaFile.Id).Execute();
        //}



        public void OverWriteMima(string content)
        {
            var copyName = DateTime.Now.ToString(".yyyy-dd-M--HH-mm-ss-fff");
            var newFile = new DriveData.File()
            {
                Name = "cherry.data" + copyName,
                Parents = new List<string>(new string[] { _mimaFolder.Id})
            };

            // File's new metadata.
            newFile.Description = DateTime.UtcNow.ToLongTimeString();
            newFile.MimeType = "text/plain";

            using (Stream s = GenerateStreamFromString(content))
            {
                // Send the request to the API.
                var request = 
                    _service.Files.Create(newFile, s, "text/plain");
                var t = request.Upload();
                var updatedFile = request.ResponseBody;
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        public async Task<DriveData.File> GetLatest()
        {
            var files = await EnumerateFiles(rootFolder: "mima");
            s_logger.Debug($"enumerate mima returns {files?.Count()}");
            var query = files?.Where(x => x.Name.ToLower().StartsWith("cherry.data"))
                .OrderBy(x => x.Name).LastOrDefault();
            s_logger.Debug($"cherry pick result {query?.Name}");
            return query;
        }

        public async Task<IEnumerable<DriveData.File>> EnumerateFiles(uint maxFileCounts = MaxListFileCount, string rootFolder = null)
        {
            if (maxFileCounts == 0)
            {
                throw new ArgumentNullException(nameof(maxFileCounts));
            }

            // TODO: figure "and title = {rootFolder}"
            var folders = await InternalEnumerateFiles($"mimeType = 'application/vnd.google-apps.folder'");
            var folder = folders.Where(x => x.Name == rootFolder).FirstOrDefault();
            if (folder == null)
            {
                s_logger.Error($"Do not find mima folder");
                return null;
            }

            _mimaFolder = folder;

            var files = await InternalEnumerateFiles($"'{folder.Id}' in parents");
            return files;
        }

        public async Task<string> Download(DriveData.File file)
        {
            s_logger.Debug($"Download {file.Name}");

            // TODO: verify File type.
            using (var stream = await DownloadFile(file.Id))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private async Task<IEnumerable<DriveData.File>> InternalEnumerateFiles(string query)
        {
            List<DriveData.File> results = new List<DriveData.File>();
            string pageToken = null;
            var listRequest = _service.Files.List();
            listRequest.PageSize = ListFileBatchFileCount;
            listRequest.Q = query;
            do
            {
                listRequest.PageToken = pageToken;
                var response = await listRequest.ExecuteAsync();
                results.AddRange(response.Files);
                pageToken = response.NextPageToken;
            } while (!String.IsNullOrWhiteSpace(pageToken));

            return results;
        }

        private async Task<MemoryStream> DownloadFile(string fileId)
        {
            var request = _service.Files.Get(fileId);
            var stream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                Debug.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                s_logger.Info($"Download complete. file_id {fileId}");
                                Debug.WriteLine("Download complete.");
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                s_logger.Error($"Download failed. file_id {fileId}");
                                Debug.WriteLine("Download failed.");
                                break;
                            }
                    }
                };
            await request.DownloadAsync(stream);
            return stream;
        }
    }
}