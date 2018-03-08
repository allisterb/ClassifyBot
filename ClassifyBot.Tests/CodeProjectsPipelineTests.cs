using System;
using System.Collections.Generic;
using System.IO;

using Xunit;
using ClassifyBot.Example.CodeProject.LanguageDetector;

namespace ClassifyBot.Tests
{
    public class CodeProjectPipelineTests
    {
        [Fact(DisplayName = "Can create and use LanguageDataExtractor.")]
        public void ExtractorTests()
        {
            StageResult r =  Driver.MarshalOptionsForStage<LanguageDataExtractor>(new string[] { "extract-langdata", "-f", "foo.json"}, out LanguageDataExtractor extractor, out string optionsHelp);
            Assert.Equal(StageResult.CREATED, r);
        }
    }
}
