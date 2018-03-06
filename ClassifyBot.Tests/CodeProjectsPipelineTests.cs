using System;
using System.Collections.Generic;
using System.IO;

using Xunit;
using ClassifyBot.Pipeline.CodeProject.LanguageDetector;

namespace ClassifyBot.Tests
{
    public class CodeProjectPipelineTests
    {
        [Fact(DisplayName = "Can instantiate LanguageDataExtractor.")]
        public void ExtractorTests()
        {
            FileInfo file = new FileInfo("ExtractorTest.json");
            LanguageDataExtractor e =  Driver.MarshalOptionsForStage<LanguageDataExtractor>(new string[] { "extract-langdata", "-f", "foo.json"}, out StageResult result, out string optionsHelp);
            int n = e.Extract();
        }
    }
}
