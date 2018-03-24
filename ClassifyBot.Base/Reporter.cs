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
    public abstract class Reporter : Stage, IReporter
    {
        #region Constructors
        public Reporter() : base()
        {
            Contract.Requires(!ClassifierResultsFileName.IsEmpty());
            Contract.Requires(!OutputFileName.IsEmpty());
        }
        #endregion

        #region Overriden members
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
            if ((r = Report()) != StageResult.SUCCESS)
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
            Contract.Requires(ClassStatisticsFile != null && ClassStatisticsFile == null);
            Contract.Requires(ClassifierResultsFile != null && ClassifierResultsFile == null);
            if (!ClassStatisticsFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (!ClassifierResultsFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (OutputFile.Exists && !OverwriteOutputFile)
            {
                Error("The output file {0} exists but the overwrite option was not specified.", OutputFile.FullName);
                return StageResult.OUTPUT_ERROR;
            }
            else if (OutputFile.Exists)
            {
                Warn("Output file {0} exists and will be overwritten.", OutputFile.FullName);
            }
            return StageResult.SUCCESS;
        }

        protected override StageResult Read()
        {
            if (ClassifierResultsFile.Extension == ".gz")
            {
                using (GZipStream gzs = new GZipStream(ClassifierResultsFile.OpenRead(), CompressionMode.Decompress))
                using (StreamReader r = new StreamReader(gzs))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ClassifierResults = serializer.Deserialize<List<ClassifierResult>>(reader);
                }
            }
            else
            {
                using (StreamReader r = new StreamReader(ClassifierResultsFile.OpenRead()))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ClassifierResults = serializer.Deserialize<List<ClassifierResult>>(reader);
                }
            }
            if (ClassifierResults == null || ClassifierResults.Count == 0)
            {
                Error("Did not read any records from {0}.", ClassifierResultsFile.FullName);
                return StageResult.INPUT_ERROR;
            }
            else
            {
                Info("Read {0} records from {1}.", ClassifierResults.Count, ClassifierResultsFile.FullName);
                return StageResult.SUCCESS;
            }
        }

        protected override StageResult Process() => Report();
        #endregion

        #region Abstract members
        protected abstract StageResult Report();
        #endregion

        #region Properties
        public FileInfo ClassStatisticsFile => ClassStatisticsFileName.IsEmpty() ? null : new FileInfo(ClassStatisticsFileName);

        public FileInfo ClassifierResultsFile => ClassifierResultsFileName.IsEmpty() ? null : new FileInfo(ClassifierResultsFileName);

        public FileInfo OutputFile => OutputFileName.IsEmpty() ? null : new FileInfo(OutputFileName);

        public virtual List<ClassStatistic> ClassStatistics { get; protected set; } = new List<ClassStatistic>();

        public virtual List<ClassifierResult> ClassifierResults { get; protected set; } = new List<ClassifierResult>();

        //[Option('s', "stats-file", Required = false, HelpText = "Statistics data file name for reporting.")]
        public virtual string ClassStatisticsFileName { get; set; }

        [Option('i', "input-file", Required = true, HelpText = "Input classifier results data file name for reporting.")]
        public virtual string ClassifierResultsFileName { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output file name for report.")]
        public virtual string OutputFileName { get; set; }
        #endregion

    }
}
