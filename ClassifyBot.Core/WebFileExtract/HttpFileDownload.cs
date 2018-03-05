using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using Serilog;

namespace ClassifyBot
{
    public class HttpFileDownload
    {
        #region Constructors
        public HttpFileDownload(string url, FileInfo local_file,
            DownloadProgressChangedEventHandler progress_changed_event_handler = null,
            Action<object, DownloadProgressChangedEventArgs> progress_changed_action = null,
            string user_agent = null)
        {
            this.url = new Uri(url);
            this.local_file = local_file;
            this.user_agent = string.IsNullOrEmpty(user_agent) ? "ClassifyBot/{0}".F(Version) : user_agent;
            this.progress_changed_event_handler = progress_changed_event_handler;           
        }
        #endregion

        #region Properties
        public Dictionary<string, IEnumerable<string>> Headers
        {
            get
            {
                return this._Headers;
            }
            set
            {
                if (value.ContainsKey("ContentLength") && value["ContentLength"] != null)
                {
                    long s;
                    if (Int64.TryParse( (string) value["ContentLength"].First(), out s))
                    {
                        this.Size = s;
                        L.Debug("Set Size property to: {0}.", this.Size);
                    }
                    else
                    {
                        L.Error("Could not parse ContentLength header as a number: {0}.", (string)value["ContentLength"].First());
                    }
                }
                if (value.ContainsKey("LastModified") && value["LastModified"] != null)
                {
                    try
                    {
                        this.LastModified = DateTime.Parse((string)value["LastModfied"].First());
                        L.Debug("Set LastModified property to: {0}.", this.LastModified);
                    }
                    catch (FormatException)
                    {
                        L.Debug("Could not parse LastModified header as a date: {0}.", (string)value["LastModified"].First());
                    }
                }
                this._Headers = value;
            }
        }
        #endregion

        #region Methods
        public Task StartTask()
        {
            if (this.Headers == null)
            {
                this.GetHeaders();
            }
            using (WebClient client = new WebClient())
            {
                client.BaseAddress = this.url.GetLeftPart(UriPartial.Authority);
                client.Headers.Add(HttpRequestHeader.UserAgent, this.user_agent);
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                if (this.progress_changed_event_handler != null)
                {
                    client.DownloadProgressChanged += this.progress_changed_event_handler;
                }
                else
                {
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                }
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                return client.DownloadFileTaskAsync(this.url, this.local_file.FullName); 
            }
        }

        public Dictionary<string, IEnumerable<string>> GetHeaders()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.url.GetLeftPart(UriPartial.Authority));
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, this.url);
                HttpResponseMessage response = client.SendAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    L.Debug("Got status {0} retrieving headers for url {1}. {2} headers received.", response.StatusCode, this.url, response.Headers.Count());
                    this.Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value);
                    return this.Headers;
                }
                else
                {
                    L.Debug("Got status {0}, reason phrase {1} retrieving headers for url {2}.", response.StatusCode, response.ReasonPhrase, this.url);
                    return null;
                }
                
            }
        }
        #endregion

        #region Event handlers
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            WebClient client = (WebClient)sender;
            this.TotalBytesToReceive = e.TotalBytesToReceive;
            this.TotalBytesReceived = e.TotalBytesToReceive;
            this.ProgressPercentage = e.ProgressPercentage;
            L.Information("Downloaded {0} bytes of {1} total. {2}% done", TotalBytesReceived, TotalBytesReceived + TotalBytesToReceive, ProgressPercentage);
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                this.CompletedSuccessfully = false;
                this.Error = e.Error;
                L.Error(this.Error, "Failed to download file from Url {0} to {1}.", url, local_file);
            }
            else
            {
                this.CompletedSuccessfully = true;
                L.Information("Successfully downloaded file from Url {0} to {1}.", url, local_file);
            }
        }
        #endregion

        #region Fields
        public Uri url;
        public FileInfo local_file;
        public string user_agent;
        public DownloadProgressChangedEventHandler progress_changed_event_handler;
        public DownloadDataCompletedEventHandler completed_event_handler;
        public long? Size = null;
        public DateTime? LastModified = null;
        public long TotalBytesToReceive = 0;
        public long TotalBytesReceived = 0;
        public int ProgressPercentage = 0;
        public bool CompletedSuccessfully = false;
        public Exception Error = null;
        public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        private static ILogger L = Log.ForContext<HttpFileDownload>();
        private Dictionary<string, IEnumerable<string>> _Headers;
        #endregion
    }
}
