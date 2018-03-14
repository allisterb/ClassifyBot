using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Serialization;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    public class LanguageItem : Record<string>
    {
        public LanguageItem(int lineNo, string languageName, string languageText) : base(lineNo, (languageName, 1), ("TEXT", languageText)) {}
    }
    
}
