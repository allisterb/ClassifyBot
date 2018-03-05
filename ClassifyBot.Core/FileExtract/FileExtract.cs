using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

using Serilog;
using CommandLine;

namespace ClassifyBot
{

    public abstract class FileExtract<TRecord, TFeature> : ExtractStage<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public FileExtract() : base()
        {
            Contract.Requires(InputFile != null && InputFile.Exists);
        }

        public FileExtract(string inputFileName) : base()
        {
            InputFileName = inputFileName;
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamReader, IEnumerable<TRecord>> ReadFileStream { get; }
        #endregion

        #region Properties
        [Option('i', "input-file", Required = true, HelpText = "Input data file name for dataset. A file with a .zip or .gz or .tar.gz extension will be automatically decompressed.")]
        public virtual string InputFileName { get; set; }

        public FileInfo InputFile => InputFileName.Empty() ? null : new FileInfo(InputFileName);
        #endregion



        #region Implemented members
        public override int Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            if (InputFile.Extension == ".gz")
            {
                using (GZipStream gzs = new GZipStream(InputFile.OpenRead(), CompressionMode.Decompress))
                using (StreamReader r = new StreamReader(gzs))
                {
                    ExtractedRecords.AddRange(ReadFileStream(L, r));
                }

            }
            return ExtractedRecords.Count;
        }
        #endregion
    }
}
