using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using CsvHelper;
using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;

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
        protected override Func<Loader<Comment, string>, StreamWriter, List<Comment>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (loader, sw, records, options) =>
        {
            using (Operation writeOp = L.BeginOperation("Writing records to file"))
            {
                using (CsvWriter csv = new CsvWriter(sw))
                {
                    SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                    string[] selectedFeatures = { "TOKEN", "LEXICAL", "WORDS" };
                    for (int i = 0; i < records.Count; i++)
                    {
                        Comment record = records[i];
                        string c = "{0}{1}{2}{3}{4}{5}".F(record.Labels[0].Item2, record.Labels[1].Item2, record.Labels[2].Item2, record.Labels[3].Item2, 
                        record.Labels[4].Item2, record.Labels[5].Item2);
                        csv.WriteField(record.Labels[0].Item2);
                        for (int j = 0; j < selectedFeatures.Length; j++)
                        {
                            IEnumerable<string> features = record.Features
                             .Where(f => f.Item1 == selectedFeatures[j])
                             .Select(f => f.Item2);
                            if (features.Count() > 0)
                            {
                                csv.WriteField(string.Join(" ", features));
                            }
                            else
                            {
                                csv.WriteField(string.Empty);
                            }
                        }

                        string sentiment = record.Features
                            .Where(f => f.Item1 == "SENTIMENT")
                            .Select(f => f.Item2)
                            .FirstOrDefault();
                        if (sentiment != null)
                        {
                            if (Single.TryParse(sentiment, out float s))
                            {
                                csv.WriteField(s);
                            }
                            else
                            {

                                L.Warning("Sentiment feature for {0} {1} is not numeric: {2}", record._Id, record.Id, sentiment);
                                csv.WriteField(0);
                            }
                        }
                        else
                        {
                            csv.WriteField(0);
                        }

                        if (!record.Id.IsEmpty())
                        {
                            csv.WriteField(record.Id);
                        }
                        if (record._Id.HasValue)
                        {
                            csv.WriteField(record._Id);
                        }
                        csv.NextRecord();
                        if ((i + 1) % 10000 == 0)
                        {
                            Log.Information("Wrote {0} records to file.", i + 1);
                        }
                    }
                }
                writeOp.Complete();
                return StageResult.SUCCESS;
            }

        };

        protected override StageResult Cleanup() => StageResult.SUCCESS;
        #endregion

        #region Properties
        [Option('L', "label", Required = false, HelpText = "The index of the label to write to files.")]
        public int Label { get; set; }
        #endregion
    }
}
