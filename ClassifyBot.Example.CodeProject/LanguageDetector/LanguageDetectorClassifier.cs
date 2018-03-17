using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-train", HelpText = "Train a classifier for the LanguageSamples dataset.")]
    public class LanguageDetectorClassifier : StanfordNLPClassifier<LanguageItem, string>
    {
    }
}
