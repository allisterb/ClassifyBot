using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public class RCommand : Command
    {
        #region Constructors
        public RCommand(string javaHome, string classPath, string className, string javaOptions, params object[] options) : base(javaHome, "java", options)
        {
            this.RHome = WorkingDirectory;
            this.ClassPath = classPath;
            this.ClassName = className;
            CommandOptions = new List<object>();
            CommandOptions.Add(javaOptions);
            CommandOptions.Add("-cp");
            CommandOptions.Add(ClassPath);
            CommandOptions.Add(ClassName);
            CommandOptions.AddRange(options);
        }
        #endregion

        #region Properties
        public string RHome { get; protected set; }
        public string ClassPath { get; protected set; }
        public string ClassName { get; protected set; }
        #endregion

        #region Fields

        #endregion
    }
}

