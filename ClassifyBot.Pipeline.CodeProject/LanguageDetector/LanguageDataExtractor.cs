using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassifyBot.Pipeline.CodeProject.LanguageDetector
{
    public class LanguageDataExtractor : WebFileExtractor<LanguageItem, string>
    {
        public LanguageDataExtractor(FileInfo outputFile, bool overwrite, bool append, Dictionary<string, object> options) : base(outputFile, overwrite, append, options) { }
    }
}
