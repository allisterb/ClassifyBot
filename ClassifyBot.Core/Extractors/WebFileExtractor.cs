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
    public abstract class WebFileExtractor<TRecord, TFeature> : FileExtractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public WebFileExtractor() : base("{0}-clbot-web-extract-dl.tmp".F(DateTime.Now.Ticks))
        {
            Contract.Requires(!InputFileUrl.Empty());
            
        }

        public WebFileExtractor(string _inputFileUrl) : base("{0}-clbot-web-extract-dl.tmp".F(DateTime.Now.Ticks))
        {
            InputFileUrl = _inputFileUrl;
            if (Uri.TryCreate(InputFileUrl, UriKind.RelativeOrAbsolute, out Uri result))
            {
                InputFileUri = result;
            }
            else throw new ArgumentException("The input file Url {0} is not a valid Uri.".F(InputFileUrl));
        }
        #endregion

        #region Overriden members
        protected override StageResult Init()
        {
            if (Uri.TryCreate(InputFileUrl, UriKind.RelativeOrAbsolute, out Uri result))
            {
                InputFileUri = result;
                return StageResult.SUCCESS;
            }
            else
            {
                Error("The input file Url {0} is not a valid Uri.".F(InputFileUrl));
                return StageResult.INPUT_ERROR;
            }
        }

        protected override StageResult Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            Contract.Requires(InputFileUri != null);
            if (InputFileUri.Segments.Any(s => s.EndsWith(".zip")))
            {
                InputFileName += ".zip";
            }
            else if (InputFileUri.Segments.Any(s => s.EndsWith(".gz")))
            {
                InputFileName += ".gz";
            }
            else if (InputFileUri.Segments.Any(s => s.EndsWith(".tar.gz")))
            {
                InputFileName += ".tar.gz";
            }
            FileDownload = new HttpFileDownload(InputFileUri.ToString(), TempFile);
            FileDownloadTask = FileDownload.StartTask();
            try
            {
                FileDownloadTask.Wait();
            }
            catch (Exception e)
            {
                Error(e, "Exception throw attempting to download file from Url {0} to {1}.", InputFileUri.ToString(), TempFile);
                return StageResult.INPUT_ERROR;
            }

            if (!FileDownload.CompletedSuccessfully)
            {
                Error("Failed to download file from Url {0} to {1}.", InputFileUri.ToString(), TempFile);
                return StageResult.INPUT_ERROR;
            }
            return base.Extract();  
        }

        protected override StageResult Cleanup()
        {
            InputFile.Delete();
            L.Debug("Deleted temporary input file {0}.", InputFileName);
            return StageResult.SUCCESS;
        }
        #endregion

        #region Properties
        [Option('u', "url", Required = true, HelpText = "Input data file Url. A file with a .zip or .gz or .tar.gz extension will be automatically decompressed.")]
        public virtual string InputFileUrl { get; set; }

        public Uri InputFileUri { get; protected set; }

        [Option('i', "input-file", Required = false, Hidden = true, HelpText = "Input data file name for dataset. A file with a .zip or .gz or .tar.gz extension will be automatically decompressed.")]
        public override string InputFileName { get; set; }

        public FileInfo TempFile => InputFile;

        public HttpFileDownload FileDownload { get; protected set; }

        public Task FileDownloadTask { get; protected set; }
        #endregion
    }
}
