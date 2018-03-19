using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;
using Serilog;

namespace ClassifyBot
{
    public abstract class CsvFileReporter : FileReporter
    {
        #region Constructors
        public CsvFileReporter() : base() {}
        #endregion

        #region Overridden members
        protected override StageResult Init()
        {
            WriterOptions.Add("HasHeaderRecord", true);
            return StageResult.SUCCESS;
        }

        protected override Func<ILogger, StreamWriter, List<ClassifierResult>, Dictionary<string, object>, StageResult> WriteResultsToFileStream { get; } = (L, sw, results, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                csv.WriteRecords(results);
                return StageResult.SUCCESS;
            }
        };

        #endregion
    }
}
