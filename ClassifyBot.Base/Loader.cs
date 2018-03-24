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
        #region Constructors
        public Loader()
        {
            Contract.Requires(!OutputFilePrefix.IsEmpty());
            Contract.Requires(!TrainingFileName.IsEmpty());
            Contract.Requires(!TestFileName.IsEmpty());
        }
        #endregion

        #region Overidden members
        public override StageResult Run(Dictionary<string, object> options = null)
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
            if ((r = Write()) != StageResult.SUCCESS)
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
                Error("The training data file {0} exists but the overwrite option was not specified.", TrainingFile.FullName);
                return StageResult.OUTPUT_ERROR;
            }
            else if (TrainingFile.Exists)
            {
                Warn("Training data file {0} exists and will be overwritten.", TrainingFile.FullName);
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

        protected override StageResult Read()
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

        protected override StageResult Process() => Load();

        protected override StageResult Write()
        {
            Contract.Requires(TrainingRecords.Count > 0);
            Contract.Requires(TestRecords.Count > 0);
            StageResult r = StageResult.OUTPUT_ERROR;
            if ((r = Save(TrainingFile, TrainingRecords)) != StageResult.SUCCESS)
            {
                return StageResult.OUTPUT_ERROR;
            }
            Info("Wrote {0} training records to file {1}.", TrainingRecords.Count, TrainingFile.FullName);
            if ((r = Save(TestFile, TestRecords)) != StageResult.SUCCESS)
            {
                return StageResult.OUTPUT_ERROR;
            }
            Info("Wrote {0} test records to file {1}.", TestRecords.Count, TestFile.FullName);
            return StageResult.SUCCESS;
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamWriter, List<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; }
        #endregion

        #region Properties
        public FileInfo TrainingFile => TrainingFileName.IsEmpty() ? null : new FileInfo(TrainingFileName);

        public FileInfo TestFile => TestFileName.IsEmpty() ? null : new FileInfo(TestFileName);

        public FileInfo InputFile => InputFileName.IsEmpty() ? null : new FileInfo(InputFileName);

        public List<TRecord> TrainingRecords { get; protected set; } = new List<TRecord>();

        public List<TRecord> TestRecords { get; protected set; } = new List<TRecord>();

        public List<TRecord> InputRecords { get; protected set; } = new List<TRecord>();

        public string TrainingFileName => OutputFilePrefix + ".train.tsv";

        public string TestFileName => OutputFilePrefix + ".test.tsv";

        [Option('i', "input-file", Required = true, HelpText = "Input data file name for stage operation.")]
        public virtual string InputFileName { get; set; }

        [Option('P', "output-prefix", Required = true, HelpText = "Output data file name prefix. Training and test data files will be created with this prefix.")]
        public string OutputFilePrefix { get; set; }

        [Option('s', "split", Required = false, HelpText = "Split the input dataset into training/test datasets with this ratio.", Default = 8)]
        public int TrainingTestSplit { get; set; }
        #endregion

        #region Methods
        protected virtual StageResult Load(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            Contract.Requires(InputRecords.Count > 0);
            float s = TrainingTestSplit;
            float c = InputRecords.Count;
            int splitCount = (int)((1f / s) * c);
            for (int i = 0; i < splitCount; i++)
            {
                TestRecords.Add(InputRecords[i]);
            }
            for (int i = splitCount; i < InputRecords.Count; i++)
            {
                TrainingRecords.Add(InputRecords[i]);
            }
            return StageResult.SUCCESS;
        }

        protected virtual StageResult Save(FileInfo file, List<TRecord> records)
        {
            Contract.Requires(records != null);
            if (records.Count == 0)
            {
                Error("0 records found to write to file {0}.", file.FullName);
                return StageResult.INPUT_ERROR;
            }
            StageResult r = StageResult.OUTPUT_ERROR;
            if (!CompressOutputFile)
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    r = WriteFileStream(L, sw, records, WriterOptions);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Create))
                using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gzs))
                {
                    r = WriteFileStream(L, sw, records, WriterOptions);
                }

            }
            return r;
        }
        #endregion
    }
}
