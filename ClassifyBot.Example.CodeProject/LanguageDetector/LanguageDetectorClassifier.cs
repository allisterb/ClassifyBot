using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-train", HelpText = "Train a classifier for the LanguageSamples dataset using the Stanford NLP classifier.")]
    public class LanguageDetectorClassifier : StanfordNLPClassifier<LanguageItem, string>
    {
        #region Overriden members
        public override Dictionary<string, object> ClassifierProperties { get; } = new Dictionary<string, object>()
        {
            {"1.useSplitWords", true },
            {"1.splitWordsRegexp", "\\\\s+" },
            {"2.useSplitWords", true },
            {"2.splitWordsRegexp", "\\\\s+" },
            {"2.useAllSplitWordPairs", true },
        };
        #endregion
    }
}
