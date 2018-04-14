using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-label", HelpText = "Label the ")]
    public class LanguageItemAnnotator : WunderkindLabelAnnotator<LanguageItem, string>
    {
    }
}
