using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public class PythonCommand : Command
    {
        #region Constructors
        public PythonCommand(string interpreterPath, string scriptName, string pythonOptions, params object[] options) : base(interpreterPath, options)
        {
            this.InterpreterPath = interpreterPath;
            this.ScriptName = scriptName;
            CommandOptions = new List<object>();
            CommandOptions.Add(pythonOptions);
            CommandOptions.Add(ScriptName);
            CommandOptions.AddRange(options);
        }
        #endregion

        #region Properties
        public string InterpreterPath { get; protected set; }
        public string ModulesPath { get; protected set; }
        public string ScriptName { get; protected set; }
        #endregion

        #region Fields

        #endregion
    }
}
    
