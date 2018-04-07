using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CommandLine;
using SerilogTimings;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-features", HelpText = "Select features from language samples data.")]
    public class LanguageSamplesSelectFeatures : Transformer<LanguageItem, string>
    {
        #region Overriden members
        protected override Func<Transformer<LanguageItem, string>, Dictionary<string, object>, LanguageItem, LanguageItem> TransformInputToOutput { get; } = (t, options, input) =>
        {
            LanguageSamplesSelectFeatures l = (LanguageSamplesSelectFeatures) t;
            
            string text = input.Features[0].Item2.Trim();
            Regex doubleQuote = new Regex("\\\".*?\\\"", RegexOptions.Compiled);
            Regex singleQuote = new Regex("\\\'.*?\\\'", RegexOptions.Compiled);
            text = singleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any quote string literals
            text = doubleQuote.Replace(text, new MatchEvaluator(ReplaceStringLiteral)); //Remove any doublequote string literals
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");

            LanguageItem output = new LanguageItem(input._Id.Value, input.Labels[0].Item1, text);

            //lexical features
            string lexicalFeature = string.Empty;

            Regex semiColon = new Regex(";\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex curlyBrace = new Regex("\\{\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex at = new Regex("^\\s*\\@\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex hashComment = new Regex("#.*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex doubleSlashComment = new Regex("//.*?$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex markupElement = new Regex("<\\/\\S+>", RegexOptions.Compiled);
            Regex colonStartBlock = new Regex(".+\\:\\s*$", RegexOptions.Compiled);
            Regex doublecolonNamespace = new Regex("\\w+\\:\\:\\w+", RegexOptions.Compiled);

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

            if (markupElement.IsMatch(text))
            {
                lexicalFeature += "MARKUP" + " ";
            }

            if (at.IsMatch(text))
            {
                lexicalFeature += "AT" + " ";
            }

            if (colonStartBlock.IsMatch(text))
            {
                lexicalFeature += "COLON_START_BLOCK" + " ";
            }

            if (doublecolonNamespace.IsMatch(text))
            {
                lexicalFeature += "DOUBLE_COLON_NAMESPACE" + " ";
            }

            output.Features.Add(("LEXICAL", lexicalFeature.Trim()));

            if (!l.WithSyntaxFeatures)
            {
                goto done;
            }

            //syntax features
            string syntacticFeature = string.Empty;

            Regex varDecl = new Regex("\\s*var\\s+\\S+\\s+\\=\\s\\S+", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex letDef = new Regex("\\s*let\\s+\\w+\\s+\\=\\s\\w+", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex defBlock = new Regex("^\\s*(def|for|try|while)\\s+\\S+\\s*\\:\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex propertyAccessor = new Regex("\\w+\\s+\\{get;", RegexOptions.Compiled);
            Regex accessModifier = new Regex("^\\s*(public|private|protected|internal|friend)\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex classExtendsDecl = new Regex("^\\s*class\\s+\\S+\\s+\\extends\\s+\\w+", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex classColonDecl = new Regex("\\s*class\\s+\\w+\\s*\\:\\s*\\w+", RegexOptions.Compiled | RegexOptions.Multiline);//^\s*class\s+\w+\s*\(\w+\)
            Regex classBracketsDecl = new Regex("^\\s*class\\s+\\w+\\s*\\(\\w+\\)", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex fromImportDecl = new Regex("^\\s*from\\s+\\S+\\s+import\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex importAsDecl = new Regex("^\\s*import\\s+\\S+\\s+as\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);
            Regex newKeywordDecl = new Regex("\\w+\\=\\s*new\\s+\\w+", RegexOptions.Compiled);
            Regex usingKeywordDecl = new Regex("^\\s*using\\s+\\w+?", RegexOptions.Compiled | RegexOptions.Multiline);


            if (varDecl.IsMatch(text))
            {
                syntacticFeature += "VAR_DECL" + " ";
            }

            if (letDef.IsMatch(text))
            {
                syntacticFeature += "LET_DEF" + " ";
            }

            if (defBlock.IsMatch(text))
            {
                syntacticFeature += "DEF_BLOCK" + " ";
            }

            if (accessModifier.IsMatch(text))
            {
                syntacticFeature += "ACCESS_MODIFIER" + " ";
            }

            if (propertyAccessor.IsMatch(text))
            {
                syntacticFeature += "PROP_DECL" + " ";
            }

            if (classExtendsDecl.IsMatch(text))
            {
                syntacticFeature += "CLASS_EXTENDS_DECL" + " ";
            }

            if (classColonDecl.IsMatch(text))
            {
                syntacticFeature += "CLASS_COLON_DECL" + " ";
            }

            if (classBracketsDecl.IsMatch(text))
            {
                syntacticFeature += "CLASS_BRACKETS_DECL" + " ";
            }

            if (fromImportDecl.IsMatch(text))
            {
                syntacticFeature += "FROM_IMPORT_DECL" + " ";
            }

            if (importAsDecl.IsMatch(text))
            {
                syntacticFeature += "IMPORT_AS_DECL" + " ";
            }

            if (newKeywordDecl.IsMatch(text))
            {
                syntacticFeature += "NEW_KEYWORD_DECL" + " ";
            }

            if (usingKeywordDecl.IsMatch(text))
            {
                syntacticFeature += "USING_KEYWORD_DECL" + " ";
            }

            output.Features.Add(("SYNTACTIC", syntacticFeature.Trim()));

            done:
            return output;
        };

        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            FeatureMap.Add(0, "TEXT");
            FeatureMap.Add(1, "LEXICAL");
            FeatureMap.Add(2, "SYNTACTIC");
            if (WithSyntaxFeatures)
            {
                Info("Selecting syntactic features.");
            }
            return StageResult.SUCCESS;
        }

        protected override StageResult Cleanup() => StageResult.SUCCESS;
        #endregion

        #region Properties
        [Option("with-syntax", HelpText = "Extract syntax features from language sample.", Required = false)]
        public bool WithSyntaxFeatures { get; set; }
        #endregion

        #region Methods
        private static string ReplaceStringLiteral(Match m)
        {
            
            return string.Empty;
        }
        #endregion
    }
}
