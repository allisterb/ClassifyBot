using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using Serilog;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-to-tsv", HelpText = "Transform extracted language data into a TSV file.")]
    public class LanguageDataTransformToTsv : TransformToCsvFile<LanguageItem, string>
    {
        #region Constructor
        public LanguageDataTransformToTsv() : base("\t") {}
        #endregion

        #region Overriden members
        protected override Func<ILogger, Dictionary<string, object>, LanguageItem, LanguageItem> TransformInputToOutput { get; } = (logger, options, input) =>
        {
            string text = input.Features[0].Trim();
            StringBuilder textBuilder = new StringBuilder(input.Features[0].Trim());

            Regex doubleQuote = new Regex("\\\".*?\\\"", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex singleQuote = new Regex("\\\'.*?\\\"", RegexOptions.Compiled | RegexOptions.Multiline);
            text = text.Replace('\t', ' '); //Remove tabs
            text = text.Replace("\r\n", " "); // Replace Windows line breaks with space
            text = text.Replace('\n', ' '); // Replace Linux line breaks with space
            text = singleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any quote string literals
            text = doubleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any doublequote string literals
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");
            LanguageItem output = new LanguageItem(input.Id.Value, input.Label, text);
            if (Regex.IsMatch(text, "{.*?}"))
            {
                output.Features.Add(FeatureMap[1]);
            }
            if (Regex.IsMatch(text, "; "))
            {
                output.Features.Add(FeatureMap[2]);
            }
            return output;
        };

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            if (!StageResultSuccess(base.Init(), out StageResult r)) return r;
            FeatureMap.Add(0, "TEXT");
            FeatureMap.Add(1, "HAS_CURLY_BRACES_TOKEN");
            FeatureMap.Add(2, "HAS_SEMICOLON_TOKEN");
            return StageResult.SUCCESS;
        }
        #endregion

        #region Methods
        private static string ReplaceStringLiteral(Match m)
        {
            return string.Empty;
        }
        #endregion
    }
}
