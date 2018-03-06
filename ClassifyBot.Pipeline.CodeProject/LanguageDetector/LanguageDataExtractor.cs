using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;

using CommandLine;

namespace ClassifyBot.Pipeline.CodeProject.LanguageDetector
{
    [Verb("extract-langdata", HelpText = "Download and extract language data from https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip into common JSON format.")]
    public class LanguageDataExtractor : WebFileExtract<LanguageItem, string>
    {
        #region Constructors
        public LanguageDataExtractor() : base("https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip")
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
        public override string InputFileUrl { get; set; }

        protected override Func<ILogger, StreamReader, IEnumerable<LanguageItem>> ReadFileStream { get; } = (logger, r) =>
        {
            return null;
        };
        #endregion
    }
}
