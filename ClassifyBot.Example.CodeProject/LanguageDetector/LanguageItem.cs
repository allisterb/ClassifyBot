using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot.Pipeline.CodeProject.LanguageDetector
{
    public class LanguageItem : Record<string>
    {
        public LanguageItem(int lineNo, string languageName, string languageText) : base(lineNo, languageName, languageText) {}
    }
}
