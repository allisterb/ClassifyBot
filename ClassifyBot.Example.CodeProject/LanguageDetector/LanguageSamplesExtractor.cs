using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;

using CommandLine;
using HtmlAgilityPack;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-extract", HelpText = "Download and extract language samples data from https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip into a common JSON format.")]
    public class LanguageSamplesExtractor : WebFileExtractor<LanguageItem, string>
    {
        #region Constructors
        public LanguageSamplesExtractor() : base("https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip") {}
        #endregion

        #region Overriden members
        [Option('u', "url", Required = false, Hidden = true)]
        public override string InputFileUrl { get; set; }

        protected override Func<ILogger, StreamReader, Dictionary<string, object>, List<LanguageItem>> ReadRecordsFromFileStream { get; } = (logger, r, options) =>
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(r);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//pre");
            logger.Information("Got {0} language data items from file.", nodes.Count);
            return nodes.Select(n => new LanguageItem(n.Line, n.Attributes["lang"].Value.StripUTF8BOM(), n.InnerText.StripUTF8BOM())).ToList();
        };
        #endregion
    }
}
