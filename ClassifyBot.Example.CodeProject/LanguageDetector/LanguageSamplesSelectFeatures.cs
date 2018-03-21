using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using Serilog;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-features", HelpText = "Select features from language samples data.")]
    public class LanguageSamplesSelectFeatures : Transformer<LanguageItem, string>
    {
        #region Constructor
        public LanguageSamplesSelectFeatures() : base() {}
        #endregion

        #region Overriden members
        protected override Func<ILogger, Dictionary<string, object>, LanguageItem, LanguageItem> TransformInputToOutput { get; } = (logger, options, input) =>
        {
            string text = input.Features[0].Item2.Trim();
            StringBuilder textBuilder = new StringBuilder(input.Features[0].Item2.Trim());
            Regex doubleQuote = new Regex("\\\".*?\\\"", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex singleQuote = new Regex("\\\'.*?\\\"", RegexOptions.Compiled | RegexOptions.Multiline);
            text = text.Replace('\t', ' '); //Remove tabs
            text = text.Replace("\r\n", " "); // Replace Windows line breaks with space
            text = text.Replace('\n', ' '); // Replace Linux line breaks with space
            text = singleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any quote string literals
            text = doubleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any doublequote string literals
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");
            LanguageItem output = new LanguageItem(input._Id.Value, input.Labels[0].Item1, text);
            
            string lexicalFeature = string.Empty;
            if (Regex.IsMatch(text, "{.*?}"))
            {
                lexicalFeature = lexicalFeature += FeatureMap[1] + " ";
            }
            else
            {
                lexicalFeature = lexicalFeature += "NO_" + FeatureMap[1] + " ";
            }
            if (Regex.IsMatch(text, "; "))
            {
                lexicalFeature = lexicalFeature += FeatureMap[2] + " ";
            }
            else
            {
                lexicalFeature = lexicalFeature += "NO_" + FeatureMap[2] + " ";
            }
            if (Regex.IsMatch(text, "<.*?>"))
            {
                lexicalFeature = lexicalFeature += FeatureMap[3] + " ";
            }
            else
            {
                lexicalFeature = lexicalFeature += "NO_" + FeatureMap[3] + " ";
            }
            output.Features.Add(("LEXICAL", lexicalFeature.Trim()));
            
            return output;
        };

        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            FeatureMap.Add(0, "TEXT");
            FeatureMap.Add(1, "CURLY_BRACES_TOKEN");
            FeatureMap.Add(2, "SEMICOLON_TOKEN");
            FeatureMap.Add(3, "MARKUP_TOKEN");
            return StageResult.SUCCESS;
        }

        protected override StageResult Cleanup() => StageResult.SUCCESS;
        #endregion

        #region Methods
        private static string ReplaceStringLiteral(Match m)
        {
            return string.Empty;
        }
        #endregion
    }
}
