using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot
{

    public abstract class FileExtractor<TRecord, TFeature> : Extractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public FileExtractor(FileInfo outputFile, bool overwrite, bool append, Dictionary<string, object> options) : base(outputFile, overwrite, append, options)
        {
            Contract.Requires(Options.ContainsKey("InputFile"));
            Contract.Requires(InputFile != null && InputFile.Exists);
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamReader, IEnumerable<TRecord>> ReadFileStream { get; }
        #endregion

        #region Properties
        public FileInfo InputFile { get; protected set; }

        #endregion

        #region Methods
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
