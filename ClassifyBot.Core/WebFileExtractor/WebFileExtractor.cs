using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace ClassifyBot
{

    public class WebFileExtractor<TRecord, TFeature> : Extractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public WebFileExtractor(FileInfo outputFile, bool overwrite, bool append, Dictionary<string, object> options) : base(outputFile, overwrite, append, options)
        {
            Contract.Requires(InputFileUrl != null);
        }

        #endregion

        #region Implemented methods
        public override int Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            string tempFileName = "{0}-clbot-web-extract-dl.tmp".F(DateTime.Now.Ticks);
            TempFile = new FileInfo(tempFileName);
            FileDownload = new HttpFileDownload(InputFileUrl.ToString(), TempFile);
            FileDownloadTask = FileDownload.StartTask();
            FileDownloadTask.Wait();
            return 0;
        }
        #endregion

        #region Properties
        public Uri InputFileUrl { get; protected set; }
        public FileInfo TempFile { get; protected set; }
        public HttpFileDownload FileDownload { get; protected set; }
        public Task FileDownloadTask { get; protected set; }
        #endregion
    }
}
