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
    public abstract class FileExtract<TRecord, TFeature> : Extractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public FileExtract() : base()
        {
            Contract.Requires(InputFile != null);
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

        public override FileInfo InputFile => InputFileName.Empty() ? null : new FileInfo(InputFileName);
        #endregion

        #region Implemented members
        public override StageResult Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            string[] compressedFileExtensions = new string[3] { ".zip", ".tar.gz", ".tar.bz" };
            if (compressedFileExtensions.Contains(InputFile.Extension))
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
                        ExtractedRecords.AddRange(ReadFileStream(L, r));
                    }
                }
            }
            else
            {
                Info("Reading file {0} with size {1} bytes.", InputFile.Name, InputFile.Length);
                using (FileStream f = InputFile.OpenRead())
                using (StreamReader r = new StreamReader(f))
                {
                    ExtractedRecords.AddRange(ReadFileStream(L, r));
                }

            }
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
