using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    [Verb("extract-web", HelpText = "Extract data from a remote file into a common JSON format.")]
    public class WebFileExtractorOptions : ExtractorOptions
    {
        #region Overriden members
        public override Type StageType { get; } = null;
        #endregion
    }
}
