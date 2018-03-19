using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CsvHelper;
using Serilog;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-load", HelpText = "Load language samples data into training and test TSV data files.")]
    public class LanguageSamplesLoad : LoadToCsvFile<LanguageItem, string>
    {
        #region Constructors
        public LanguageSamplesLoad() : base("\t") {}
        #endregion

        #region Overriden members
        protected override Func<ILogger, StreamWriter, List<LanguageItem>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (logger, sw, records, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                for (int i = 0; i < records.Count; i++)
                {
                    LanguageItem record = records[i];
                    csv.WriteField(record.Labels[0].Item1);
                    for (int f = 0; f < record.Features.Count; f++)
                    {
                        csv.WriteField(record.Features[f].Item2);
                    }
                    if (!record.Id.Empty())
                    {
                        csv.WriteField(record.Id);
                    }
                    else if (record._Id.HasValue)
                    {
                        csv.WriteField(record._Id);
                    }
                    csv.NextRecord();
                }
                return StageResult.SUCCESS;
            }

        };

        protected override StageResult Cleanup() => StageResult.SUCCESS;
        #endregion
    }
}
