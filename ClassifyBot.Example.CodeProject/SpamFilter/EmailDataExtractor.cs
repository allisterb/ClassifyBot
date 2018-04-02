using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog;

using CommandLine;

namespace ClassifyBot.Pipeline.CodeProject.SpamFilter
{
    //[Verb("emaildata-extract", HelpText = "Extract email data from the CodeProject site into a common JSON format.")]
    public class EmailDataExtractor : WebFileExtractor<EmailItem, string>
    {
        #region Constructors
        public EmailDataExtractor() : base("https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip")
        {
            
        }
        #endregion

        #region Overriden members
        [Option('u', "url", Required = false, Hidden = true)]
        public override string InputFileUrl => string.Empty;

        protected override Func<FileExtractor<EmailItem, string>, StreamReader, Dictionary<string, object>, List<EmailItem>> ReadRecordsFromFileStream { get; } = (extractor, r, options) =>
        {
            return null;
        };
        #endregion
    }
}
