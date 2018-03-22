using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.TCCC
{
    [Verb("tcc-classify", HelpText = "Train a classifier for the TCC dataset using the Stanford NLP classifier.")]
    public class ToxicityClassifier : StanfordNLPClassifier<Comment, string>
    {
        #region Overriden members
        public override Dictionary<string, object> ClassifierProperties { get; } = new Dictionary<string, object>()
        {
            {"useBinary", true },
            {"3.binnedLengths", "30,60,120,240"},
            { "1.useSplitWords", true },
            {"1.splitWordsRegexp", "\\\\s+" },
            {"1.useAllSplitWordPairs", true },
            {"2.useSplitWords", true },
            {"2.splitWordsRegexp", "\\\\s+" },
            {"2.useAllSplitWordPairs", true },
            {"3.useSplitWords", true },
            {"3.splitWordsRegexp", "\\\\s+" },
            {"4.realValued", true },
            {"displayedColumn", 6 }
        };
        #endregion
    }
}
