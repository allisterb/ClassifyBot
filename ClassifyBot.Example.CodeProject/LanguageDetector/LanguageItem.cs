using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    public class LanguageItem : Record<string>
    {
        public LanguageItem(int lineNo, string languageName, string languageText) : base(lineNo, languageName, languageText) {}
    }
}
