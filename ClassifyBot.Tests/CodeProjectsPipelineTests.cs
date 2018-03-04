using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

using ClassifyBot.Pipeline.CodeProject.LanguageDetector;

namespace ClassifyBot.Tests
{
    public class CodeProjectPipelineTests
    {
        public const string CPUrl = "https://www.codeproject.com/script/Contests/Uploads/1024/LanguageSamples.zip";


        [Fact(DisplayName = "WebFileExtractor test")]
        public void ExtractorTests()
        {
            FileInfo file = new FileInfo("ExtractorTest.json");
            Dictionary<string, object> ExtractorOptions = new Dictionary<string, object>() { { "InputFileUrl", new Uri(CPUrl) } };
            LanguageDataExtractor e = new LanguageDataExtractor(file, true, false, ExtractorOptions);
            int n = e.Extract();
        }
    }
}
