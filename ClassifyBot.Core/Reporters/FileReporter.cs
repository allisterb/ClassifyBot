using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public abstract class FileReporter : Reporter
    {
        #region Constructors
        public FileReporter() : base() {}
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, StreamWriter, List<ClassifierResult>, Dictionary<string, object>, StageResult> WriteResultsToFileStream { get; }
        #endregion

        #region Methds
        protected override StageResult Write()
        {
            using (FileStream f = OutputFile.OpenRead())
            using (StreamWriter sw = new StreamWriter(f))
            {
                return WriteResultsToFileStream(L, sw, ClassifierResults, WriterOptions);
            }
        }
        #endregion
    }
}
