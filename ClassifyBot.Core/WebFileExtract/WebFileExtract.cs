using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Text;

using CommandLine;
using Serilog;

namespace ClassifyBot
{
    public abstract class WebFileExtract<TRecord, TFeature> : FileExtract<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public WebFileExtract() : base("{0}-clbot-web-extract-dl.tmp".F(DateTime.Now.Ticks))
        {
            Contract.Requires(!_InputFileUrl.Empty());
            if (Uri.TryCreate(_InputFileUrl, UriKind.RelativeOrAbsolute, out Uri result))
            {
                InputFileUrl = result;
            }
            else throw new ArgumentException("The input file Url {0} is not a valid Uri.".F(_InputFileUrl));
        }

        public WebFileExtract(string _inputFileUrl) : base("{0}-clbot-web-extract-dl.tmp".F(DateTime.Now.Ticks))
        {
            _InputFileUrl = _inputFileUrl;
            if (Uri.TryCreate(_InputFileUrl, UriKind.RelativeOrAbsolute, out Uri result))
            {
                InputFileUrl = result;
            }
            else throw new ArgumentException("The input file Url {0} is not a valid Uri.".F(_InputFileUrl));
        }
        #endregion

        #region Overriden members
        public override int Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            if (InputFileUrl.Segments.Any(s => s.EndsWith(".zip")))
            {
                InputFileName += ".zip";
            }
            else if (InputFileUrl.Segments.Any(s => s.EndsWith(".gz")))
            {
                InputFileName += ".gz";
            }
            else if (InputFileUrl.Segments.Any(s => s.EndsWith(".tar.gz")))
            {
                InputFileName += ".tar.gz";
            }
            FileDownload = new HttpFileDownload(InputFileUrl.ToString(), TempFile);
            FileDownloadTask = FileDownload.StartTask();
            try
            {
                FileDownloadTask.Wait();
            }
            catch (Exception e)
            {
                L.Error(e, "Exception throw attempting to download file from Url {0} to {1}.", InputFileUrl.ToString(), TempFile);
                return -1;
            }

            if (!FileDownload.CompletedSuccessfully)
            {
                L.Error("Failed to download file from Url {0} to {1}.", InputFileUrl.ToString(), TempFile);
                return -1;
            }
            else
            {
                return base.Extract();
            }
        }
        #endregion

        #region Properties

        [Option('u', "url", Required = true, HelpText = "Input data file Url. A file with a .zip or .gz or .tar.gz extension will be automatically decompressed.")]
        public virtual string _InputFileUrl { get; set; }

        public Uri InputFileUrl { get; protected set; }

        [Option('i', "input-file", Required = false, Hidden = true, HelpText = "Input data file name for dataset. A file with a .zip or .gz or .tar.gz extension will be automatically decompressed.")]
        public override string InputFileName { get; set; }

        public FileInfo TempFile => InputFile;

        public HttpFileDownload FileDownload { get; protected set; }

        public Task FileDownloadTask { get; protected set; }
        #endregion
    }
}
