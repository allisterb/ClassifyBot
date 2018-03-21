using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CommandLine;
using CsvHelper;
using Serilog;


namespace ClassifyBot.Example.TCCC
{
    [Verb("tcc-extract", HelpText = "Extract comment data from a local TCC data file into a common JSON format.")]
    public class CommentCsvFileExtractor : FileExtractor<Comment, string>
    {
        protected override Func<ILogger, StreamReader, Dictionary<string, object>, List<Comment>> ReadRecordsFromFileStream { get; } = (logger, sr, options) =>
        {
            using (CsvReader csv = new CsvReader(sr))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                int recordLimitSize = (int)options["RecordLimitSize"];
                List<Comment> comments = new List<Comment>();
                var dataRow = new
                {
                    id = string.Empty,
                    comment_text = string.Empty,
                    toxic = default(int),
                    severe_toxic = default(int),
                    obscene = default(int),
                    threat = default(int),
                    insult = default(int),
                    identity_hate = default(int)

                };
                csv.Read();
                csv.ReadHeader();
                int i = 1;
                while (csv.Read())
                {
                    var r = csv.GetRecord(dataRow);
                    comments.Add(new Comment(i, r.id, r.comment_text, r.toxic, r.severe_toxic, r.obscene, r.threat, r.insult, r.identity_hate));
                    if (i  % 20000 == 0)
                    {
                        logger.Information("Extracted {0} records from CSV file.", i);
                    }
                    if ((recordLimitSize > 0) && (i == recordLimitSize))
                    {
                        logger.Information("Stopping extraction at record limit {0}.", i);
                        break;
                    }
                    i++;
                }
                return comments;
            }
        };

        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            WriterOptions.Add("HasHeaderRecord", true);
            return StageResult.SUCCESS;
        }

        protected override StageResult Cleanup() => StageResult.SUCCESS;
    }
}
