using System;
using System.Collections.Generic;
using System.IO;

using Xunit;
using ClassifyBot.Pipeline.CodeProject.LanguageDetector;

namespace ClassifyBot.Tests
{
    public class CodeProjectPipelineTests
    {
        [Fact(DisplayName = "LanguageDataExtractor functions properly")]
        public void ExtractorTests()
        {
            FileInfo file = new FileInfo("ExtractorTest.json");
            LanguageDataExtractor e =  Stage.MarshalOptionsForStage<LanguageDataExtractor>(new string[] { "extract", "-f", "foo.json"}, out string optionsHelp);
            int n = e.Extract();
        }
    }
}
