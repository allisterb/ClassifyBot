using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


using CommandLine;
using MeSh = Medallion.Shell;
using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;

namespace ClassifyBot
{
    public abstract class Stage
    {
        #region Constructors
        public Stage()
        {

        }
        #endregion

        #region Abstract members
        public abstract StageResult Run(Dictionary<string, object> options = null);
        protected abstract StageResult Init();
        protected abstract StageResult Read();
        protected abstract StageResult Process();
        protected abstract StageResult Write();
        protected abstract StageResult Cleanup();
        #endregion

        #region Properties
        public virtual ILogger L { get; } = Log.ForContext<Stage>();

        public static Dictionary<string, object> StageOptions { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> AdditionalOptions => ParseAdditionalOptions(AdditionalOptionsString);

        public List<FileInfo> InputFiles { get; protected set; } = new List<FileInfo>();

        public List<FileInfo> OutputFiles { get; protected set; } = new List<FileInfo>();

        public Dictionary<string, object> ReaderOptions { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> WriterOptions { get; } = new Dictionary<string, object>();

        [Option("with-debug", HelpText = "Enable debug output.", Required = false)]
        public bool WithDebugOutput { get; set; }

        [Option("with-log-file", HelpText = "Log output to a text file.", Required = false)]
        public bool WithLogFile { get; set; }

        [Option("without-console", HelpText = "Don't log output to console.", Required = false)]
        public bool WithoutConsole { get; set; }

        [Option('o', "options", HelpText = "Any additional options for the stage in the format opt1=val,opt2=val2...", Required = false)]
        public string AdditionalOptionsString
        {
            get; set;
        }

        //[Option("explicit", HelpText = "Enable explicit loading of assemblies.", Required = false)]
        public string ExplicitAssemblies { get; set; }

        [Option('w', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output data file if it exists.")]
        public virtual bool OverwriteOutputFile { get; set; }

        [Option('c', "compress", Required = false, Default = false, HelpText = "Output file will be compressed with gzip")]
        public virtual bool CompressOutputFile { get; set; }

        [Option('b', "batch", Required = false, HelpText = "Batch the number of records processed for stage operation.", Default = 0)]
        public virtual int RecordBatchSize { get; set; }

        [Option('l', "records", Required = false, HelpText = "Limit the number of records processed for stage operation.", Default = 0)]
        public virtual int RecordLimitSize { get; set; }

        [Option('p', "parallel", Required = false, HelpText = "Use parallel execution where possible.", Default = false)]
        public virtual bool ParallelExecution { get; set; }
        #endregion

        #region Methods
        [DebuggerStepThrough] protected virtual void Info(string messageTemplate, params object[] propertyValues) => L.Information(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Debug(string messageTemplate, params object[] propertyValues) => L.Debug(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(string messageTemplate, params object[] propertyValues) => L.Error(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(Exception e, string messageTemplate, params object[] propertyValues) => L.Error(e, messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Verbose(string messageTemplate, params object[] propertyValues) => L.Verbose(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Warn(string messageTemplate, params object[] propertyValues) => L.Warning(messageTemplate, propertyValues);
        [DebuggerStepThrough]
        public virtual Operation Begin(string messageTemplate, params object[] args) 
        {
            Info(messageTemplate + "...", args);
            return L.BeginOperation(messageTemplate, args);
        }

        protected static void SetPropFromDict(Type t, object o, Dictionary<string, object> p)
        {
            foreach (PropertyInfo prop in t.GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(o, p[prop.Name]);
                }
            }
        }

        protected static Dictionary<string, object> ParseAdditionalOptions(string o)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            if (o.Empty())
            {
                return options;
            }
            Regex re = new Regex(@"([^\,]+)\=([^\,]+)", RegexOptions.Compiled);
            string[] pairs = o.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in pairs)
            {
                Match m = re.Match(s);
                if (!m.Success)
                {
                    options.Add("_ERROR_", s);
                }
                else if (options.ContainsKey(m.Groups[1].Value))
                {
                    options[m.Groups[1].Value] = m.Groups[2].Value;
                }
                else
                {
                    options.Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }
            return options;
        }

        protected static bool Success(StageResult rtest, out StageResult r)
        {
            r = rtest;
            return r == StageResult.SUCCESS ? true : false;
        }

        protected bool CheckCommandStartedAndReport(Command c)
        {
            if (!c.Started)
            {
                if (c.Exception != null)
                {
                    Error(c.Exception, "The command {0} did not start.", c.Text);
                }
                else
                {
                    Error(c.Exception, "The command {0} did not start.", c.Text);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        protected bool CheckCommandSuccessAndReport(Command c)
        {
            if (!c.Success)
            {
                if (c.Exception != null)
                {
                    Error(c.Exception, "The command {0} failed: {1} {2}", c.Text, c.OutputText, c.ErrorText);
                }
                else
                {
                    Error(c.Exception, "The command {0} failed: {1} {2}", c.Text, c.OutputText, c.ErrorText);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        protected void PrintCommand(Command c)
        {
            if (AdditionalOptions.ContainsKey("PrintCommand") && (bool)AdditionalOptions["PrintCommand"] == true)
            {
                Info("Executing command {0} {1} in working directory {2}.", c.Text, string.Join(" ", c.CommandOptions), c.WorkingDirectory);
            }
        }

        #endregion

        #region Fields
        protected static string[] CompressedFileExtensions = new string[3] { ".zip", ".tar.gz", ".tar.bz" };
        #endregion
    }
}
