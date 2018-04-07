using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using CsvHelper;
using Serilog;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-load", HelpText = "Load language samples data into training and test TSV data files.")]
    public class LanguageSamplesLoader : LoadToCsvFile<LanguageItem, string>
    {
        #region Constructors
        public LanguageSamplesLoader() : base("\t") {}
        #endregion
    }
}
