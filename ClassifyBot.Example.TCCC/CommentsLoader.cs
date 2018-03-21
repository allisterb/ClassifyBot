using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CsvHelper;
using Serilog;

using CommandLine;

namespace ClassifyBot.Example.TCCC
{
    [Verb("tcc-load", HelpText = "Load comments into training and test TSV data files.")]
    public class CommentsLoader : LoadToCsvFile<Comment, string>
    {
        #region Constructors
        public CommentsLoader() : base("\t") {}
        #endregion

        #region Overriden members
        protected override Func<ILogger, StreamWriter, List<Comment>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (logger, sw, records, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                for (int i = 0; i < records.Count; i++)
                {
                    Comment record = records[i];
                    csv.WriteField(record.Labels[0].Item2);
                    string features = string.Empty;
                    float sentiment = 0;
                    for (int f = 0; f < record.Features.Count; f++)
                    {
                        if (record.Features[f].Item1 != "WORDS" && record.Features[f].Item1 != "SENTIMENT")
                        {
                            features += record.Features[f].Item2 + " ";
                        }
                        else if (record.Features[f].Item1 == "SENTIMENT")
                        {
                            sentiment = Single.Parse(record.Features[f].Item2);
                        }
                    }
                    csv.WriteField(features.TrimEnd());
                    csv.WriteField(sentiment);
                    if (!record.Id.Empty())
                    {
                        csv.WriteField(record.Id);
                    }
                    if (record._Id.HasValue)
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
