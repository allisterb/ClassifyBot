using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Text;

using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace ClassifyBot
{
    public abstract class Loader<TRecord, TFeature> : Stage, ILoader<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overidden members
        public override FileInfo InputFile => InputFileName.Empty() ? null : new FileInfo(InputFileName);

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
            if ((r = Load()) != StageResult.SUCCESS)
            {
                return r;
            }
            Cleanup();
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            Contract.Requires(InputFile != null && TrainingFile != null && TestFile != null);

            if (!InputFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }

            if (TrainingFile.Exists && !OverwriteOutputFile)
            {
                Error("The training data file {0} exists but the overwrite option was not specified.", OutputFile.FullName);
                return StageResult.OUTPUT_ERROR;
            }
            else if (TrainingFile.Exists)
            {
                Warn("Training data file {0} exists and will be overwritten.", OutputFile.FullName);
            }

            if (TestFile.Exists && !OverwriteOutputFile)
            {
                Error("The test data file {0} exists but the overwrite option was not specified.", TestFile.FullName);
                return StageResult.OUTPUT_ERROR;
            }
            else if (TestFile.Exists)
            {
                Warn("Test data file {0} exists and will be overwritten.", TestFile.FullName);
            }

            return StageResult.SUCCESS;
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamWriter, List<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; }
        #endregion

        #region Properties
        public FileInfo TrainingFile => TrainingFileName.Empty() ? null : new FileInfo(TrainingFileName);

        public FileInfo TestFile => TestFileName.Empty() ? null : new FileInfo(TestFileName);

        public List<TRecord> TrainingRecords { get; protected set; } = new List<TRecord>();

        public List<TRecord> TestRecords { get; protected set; } = new List<TRecord>();

        public List<TRecord> InputRecords { get; protected set; } = new List<TRecord>();

        public Dictionary<string, object> ReaderOptions { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> WriterOptions { get; } = new Dictionary<string, object>();

        [Option('i', "input-file", Required = true, HelpText = "Input data file name for loading.")]
        public string InputFileName { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name prefix. Training and test data files will be created with this prefix.")]
        public string OutputFilePrefix { get; set; }

        [Option('i', "train-file", Required = true, HelpText = "Input file name with training data for classifier")]
        public string TrainingFileName { get; set; }

        [Option('t', "test-file", Required = true, HelpText = "Input file name with test data for classifier")]
        public string TestFileName { get; set; }

        [Option('s', "split", Required = true, HelpText = "Split the input dataset into training/test datasets with this ratio.", Default = 0)]
        public int TrainingTestSplit { get; set; }

        [Option('w', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output data files if they exist.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('b', "batch", Required = false, HelpText = "Batch the number of records transformed.", Default = 0)]
        public int RecordBatchSize { get; set; }

        [Option('l', "records", Required = false, HelpText = "Limit the number of records transformed.", Default = 0)]
        public int RecordLimitSize { get; set; }
        #endregion

        #region Methods
        public virtual StageResult Load(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            return StageResult.SUCCESS;
        }

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
