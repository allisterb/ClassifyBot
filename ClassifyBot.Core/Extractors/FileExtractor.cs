using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

using Serilog;
using CommandLine;
using SharpCompress;
using SharpCompress.Readers;
using SharpCompress.IO;

namespace ClassifyBot
{
    public abstract class FileExtractor<TRecord, TFeature> : Extractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public FileExtractor() : base()
        {
            Contract.Requires(InputFile != null);
        }

        public FileExtractor(string inputFileName) : base()
        {
            InputFileName = inputFileName;
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamReader, Dictionary<string, object>, List<TRecord>> ReadRecordsFromFileStream { get; }
        #endregion

        #region Overridden members
        protected override StageResult Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            if (CompressedFileExtensions.Contains(InputFile.Extension))
            {
                using (FileStream stream = InputFile.OpenRead())
                using (IReader reader = ReaderFactory.Open(stream))
                {
                    bool fileFound = false;
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            fileFound = true;
                            break;
                        }
                    }
                    if (!fileFound)
                    {
                        Error("{0} has no file entries in zip archive.".F(InputFile.FullName));
                        return StageResult.INPUT_ERROR;
                    }
                    else
                    {
                        Info("Unzipping file {0} with size {1} bytes.", reader.Entry.Key, reader.Entry.Size);
                    }
                    using (Stream rs = reader.OpenEntryStream())
                    using (StreamReader r = new StreamReader(rs))
                    {
                        ExtractedRecords.AddRange(ReadRecordsFromFileStream(L, r, WriterOptions));
                    }
                }
            }
            else
            {
                Info("Reading file {0} with size {1} bytes...", InputFile.Name, InputFile.Length);
                using (FileStream f = InputFile.OpenRead())
                using (StreamReader r = new StreamReader(f))
                {
                    ExtractedRecords.AddRange(ReadRecordsFromFileStream(L, r, WriterOptions));
                }

            }
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
