using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace ClassifyBot
{
    public abstract class Transformer<TRecord, TFeature> : Stage, ITransformer<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public Transformer()
        {
            Contract.Requires(!InputFileName.Empty());
            Contract.Requires(!OutputFileName.Empty());
        }
        #endregion

        #region Overriden members
        public override StageResult Run()
        {
            StageResult r;
            if ((r = Init()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Read()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Transform()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Save()) != StageResult.SUCCESS)
            {
                return r;
            }
            Cleanup();
            return StageResult.SUCCESS;
        }

        protected override StageResult Save()
        {
            Contract.Requires(OutputRecords != null);
            if (OutputRecords.Count == 0)
            {
                Warn("0 records transformed from file {0}. Not writing to output file.", InputFile.FullName);
                return StageResult.SUCCESS;
            }
            using (FileStream fs = new FileStream(OutputFile.FullName, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                WriteFileStream(L, sw, OutputRecords, WriterOptions);
            }
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            Contract.Requires(InputFile != null && OutputFile == null);
            if (!InputFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (OutputFile.Exists && !OverwriteOutputFile)
            {
                Error("The output file {0} exists but the overwrite option was not specified.");
                return StageResult.OUTPUT_ERROR;
            }
            else if (OutputFile.Exists)
            {
                Warn("Output file {0} exists and will be overwritten.", OutputFile.FullName);
            }
            return StageResult.SUCCESS;
        }
        #endregion

        #region Abstract members
        public abstract StageResult Transform(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null);

        protected abstract Func<ILogger, StreamWriter, IEnumerable<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; }
        #endregion

        #region Properties
        public override FileInfo InputFile => InputFileName.Empty() ? null : new FileInfo(InputFileName);

        public override FileInfo OutputFile => OutputFileName.Empty() ? null : new FileInfo(OutputFileName);

        public List<TRecord> InputRecords { get; protected set; } = new List<TRecord>();

        public List<TRecord> OutputRecords { get; protected set; } = new List<TRecord>();

        public Dictionary<string, object> ReaderOptions { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> WriterOptions { get; } = new Dictionary<string, object>();

        [Option('i', "input-file", Required = true, HelpText = "Input data file name for transformation.")]
        public string InputFileName { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for transformed dataset.")]
        public string OutputFileName { get; set; }

        [Option('o', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output data file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('b', "batch", Required = false, HelpText = "Batch the number of records transformed.", Default = 0)]
        public int RecordBatchSize { get; set; }

        [Option('l', "records", Required = false, HelpText = "Limit the number of records transformed.", Default = 0)]
        public int RecordLimitSize { get; set; }
        #endregion

        #region Methods
        protected virtual StageResult Read()
        {
            if (InputFile.Extension == ".gz")
            {
                using (GZipStream gzs = new GZipStream(InputFile.OpenRead(), CompressionMode.Decompress))
                using (StreamReader r = new StreamReader(gzs))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    InputRecords = serializer.Deserialize<List<TRecord>>(reader);
                }
            }
            else
            {
                using (StreamReader r = new StreamReader(InputFile.OpenRead()))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    InputRecords = serializer.Deserialize<List<TRecord>>(reader);
                }
            }
            if (InputRecords == null || InputRecords.Count == 0)
            {
                Error("Did not read any records from {0}.", InputFile.FullName);
                return StageResult.INPUT_ERROR;
            }
            else
            {
                Info("Read {0} records from {1}.", InputRecords.Count, InputFile.FullName);
                return StageResult.SUCCESS;
            }
        }

        #endregion
    }
}
