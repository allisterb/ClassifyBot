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
    [Verb("langdata-extract-local", HelpText = "Extract language samples data from local zip file.")]
    public class LanguageSamplesLocalExtractor : FileExtractor<LanguageItem, string>
    {
        #region Constructors
        public LanguageSamplesLocalExtractor() : base() {}
        #endregion

        #region Overriden members
        protected override Func<ILogger, StreamReader, Dictionary<string, object>, List<LanguageItem>> ReadRecordsFromFileStream { get; } = (logger, r, options) =>
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(r);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//pre");
            logger.Information("Got {0} language data items from file.", nodes.Count);
            return nodes.Select(n => new LanguageItem(n.Line, n.Attributes["lang"].Value, n.InnerText.StripUTF8BOM())).ToList();
        };

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
