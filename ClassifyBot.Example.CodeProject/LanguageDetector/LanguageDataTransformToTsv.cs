using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.CodeProject.LanguageDetector
{
    [Verb("langdata-to-tsv", HelpText = "Transform extracted language data into a TSV file.")]
    public class LanguageDataTransformToTsv : TransformToCsvFile<LanguageItem, string>
    {
        #region Constructor
        public LanguageDataTransformToTsv() : base("\t")
        {
            WriterOptions.Add("HasHeaderRecord", false);
        }
        #endregion

        #region Overriden members
        public override StageResult Transform(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            OutputRecords = InputRecords;
            return StageResult.SUCCESS;
        }

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
