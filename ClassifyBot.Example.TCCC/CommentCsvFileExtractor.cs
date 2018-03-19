using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot.Example.TCCC
{
    public class CommentCsvFileExtractor : FileExtract<Comment, string>
    {
        protected override Func<ILogger, StreamReader, IEnumerable<Comment>> ReadRecordsFromFileStream { get; } = (logger, r) =>
        {
            return null;
        };

        protected override StageResult Cleanup() => StageResult.SUCCESS;
    }
}
