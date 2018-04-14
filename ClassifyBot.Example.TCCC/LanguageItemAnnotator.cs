using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace ClassifyBot.Example.TCCC
{
    [Verb("tcc-label", HelpText = "Label comments")]
    public class LanguageItemAnnotator : WunderkindLabelAnnotator<Comment, string>
    {
        #region Overriden members
        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            InterfaceOptions.Add("Title", "Toxic Comment Classification Challenge Label Annotator.");
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
