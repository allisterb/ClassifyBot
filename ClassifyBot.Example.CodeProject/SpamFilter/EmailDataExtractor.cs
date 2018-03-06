using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;

using CommandLine;

namespace ClassifyBot.Pipeline.CodeProject.SpamFilter
{
    [Verb("extract-emaildata", HelpText = "Extract email data from the CodeProject site into a common JSON format.")]
    public class EmailDataExtractor : WebFileExtract<EmailItem, string>
    {
        #region Constructors
        public EmailDataExtractor() : base("https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip")
        {
            
        }
        #endregion

        #region Overriden members
        public override StageResult Run()
        {
            Extract();
            return StageResult.SUCCESS;
        }
        [Option('u', "url", Required = false, Hidden = true)]
        public override string InputFileUrl => string.Empty;

        protected override Func<ILogger, StreamReader, IEnumerable<EmailItem>> ReadFileStream { get; } = (logger, r) =>
        {
            return null;
        };
        #endregion
    }
}
