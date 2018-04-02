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
        #region Overriden members
        protected override Func<Transformer<LanguageItem, string>, Dictionary<string, object>, LanguageItem, LanguageItem> TransformInputToOutput { get; } = (t, options, input) =>
        {
            string text = input.Features[0].Item2.Trim();
            StringBuilder textBuilder = new StringBuilder(input.Features[0].Item2.Trim());
            Regex doubleQuote = new Regex("\\\".*?\\\"", RegexOptions.Compiled);
            Regex singleQuote = new Regex("\\\'.*?\\\'", RegexOptions.Compiled);
            Regex semiColon = new Regex(";\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex at = new Regex("^\\@\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex veeAR = new Regex("^\\s*var\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex def = new Regex("^\\s*def\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex let = new Regex("^\\s*let\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex newKeword = new Regex("new\\s+\\w+?", RegexOptions.Compiled);
            Regex classKeword = new Regex("class\\s+\\w+?", RegexOptions.Compiled);
            Regex curlyBrace = new Regex("^\\s*{", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex import = new Regex("^\\s*import\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex hashComment = new Regex("#.*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex doubleSlashComment = new Regex("\\/\\/\\s*\\.*?$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex markupElement = new Regex("<\\S+\\/>", RegexOptions.Compiled);

            
            text = singleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any quote string literals
            text = doubleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any doublequote string literals
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");

            string lexicalFeature = string.Empty;

            if (hashComment.IsMatch(text))
            {
                lexicalFeature += "HASH_COMMENT" + " ";
                text = hashComment.Replace(text, new MatchEvaluator(ReplaceStringLiteral));
            }

            else if (doubleSlashComment.IsMatch(text))
            {
                lexicalFeature += "DOUBLESLASH_COMMENT" + " ";
                text = doubleSlashComment.Replace(text, new MatchEvaluator(ReplaceStringLiteral));
            }

            if (semiColon.IsMatch(text))
            {
                lexicalFeature += "SEMICOLON" + " ";
            }

            if (curlyBrace.IsMatch(text))
            {
                lexicalFeature += "CURLY_BRACE" + " ";
            }


            if (at.IsMatch(text))
            {
                lexicalFeature += "AT" + " ";
            }

            if (veeAR.IsMatch(text))
            {
                lexicalFeature += "VAR" + " ";
            }

            if (def.IsMatch(text))
            {
                lexicalFeature += "DEF" + " ";
            }

            if (let.IsMatch(text))
            {
                lexicalFeature += "LET" + " ";
            }

            if (import.IsMatch(text))
            {
                lexicalFeature += "IMPORT" + " ";
            }

            if (newKeword.IsMatch(text))
            {
                lexicalFeature += "NEW" + " ";
            }

            if (classKeword.IsMatch(text))
            {
                lexicalFeature += "CLASS" + " ";
            }

            if (markupElement.IsMatch(text))
            {
                lexicalFeature += "MARKUP" + " ";
            }

            
            LanguageItem output = new LanguageItem(input._Id.Value, input.Labels[0].Item1, text);
            
            output.Features.Add(("LEXICAL", lexicalFeature.Trim()));
            
            return output;
        };

        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            FeatureMap.Add(0, "TEXT");
            FeatureMap.Add(1, "LEXICAL");
            FeatureMap.Add(2, "SYNTACTIC");
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
