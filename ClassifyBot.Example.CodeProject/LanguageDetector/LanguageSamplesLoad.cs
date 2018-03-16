using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-load", HelpText = "Load language samples data into training and test TSV data files.")]
    public class LanguageSamplesLoad : LoadToCsvFile<LanguageItem, string>
    {
        public LanguageSamplesLoad() : base() {}
    }
}
