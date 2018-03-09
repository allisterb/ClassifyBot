using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


using CommandLine;
using Serilog;
namespace ClassifyBot
{
    public abstract class Stage
    {
        #region Constructors
        public Stage()
        {

        }
        #endregion

        #region Properties
        public virtual ILogger L { get; } = Log.ForContext<Stage>();

        public static Dictionary<string, object> StageOptions { get; } = new Dictionary<string, object>();

        public bool Initialized { get; protected set; } = false;

        public Dictionary<string, object> AdditionalOptions => ParseAdditionalOptions(AdditionalOptionsString);

        public static SortedList<int, string> FeatureMap { get; } = new SortedList<int, string>();

        [Option("debug", HelpText = "Enable debug output.", Required = false)]
        public bool DebugOutput { get; set; }

        [Option("verbose", HelpText = "Enable verbose output.", Required = false)]
        public bool VerboseOutput { get; set; }

        [Option('o', "options", HelpText = "Any additional options fior the stage in the format opt1=val,opt2=val2...", Required = false)]
        public string AdditionalOptionsString
        {
            get; set;
        }

        //[Option("explicit", HelpText = "Enable explicit loading of assemblies.", Required = false)]
        public string ExplicitAssemblies { get; set; }

        public string[] CompressedFileExtensions = new string[3] { ".zip", ".tar.gz", ".tar.bz" };
        #endregion

        #region Abstract members
        public abstract FileInfo InputFile { get; }
        public abstract FileInfo OutputFile { get; }
        public abstract StageResult Run();
        protected abstract StageResult Init();
        protected abstract StageResult Save();
        protected abstract StageResult Cleanup();
        #endregion

        #region Methods
        public virtual void Info(string messageTemplate, params object[] propertyValues) => L.Information(messageTemplate, propertyValues);
        public virtual void Debug(string messageTemplate, params object[] propertyValues) => L.Debug(messageTemplate, propertyValues);
        public virtual void Error(string messageTemplate, params object[] propertyValues) => L.Error(messageTemplate, propertyValues);
        public virtual void Error(Exception e, string messageTemplate, params object[] propertyValues) => L.Error(e, messageTemplate, propertyValues);
        public virtual void Verbose(string messageTemplate, params object[] propertyValues) => L.Verbose(messageTemplate, propertyValues);
        public virtual void Warn(string messageTemplate, params object[] propertyValues) => L.Warning(messageTemplate, propertyValues);

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
            Regex re = new Regex(@"(\w+)\=([^\,]+)", RegexOptions.Compiled);
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

        protected static bool StageResultSuccess(StageResult rtest, out StageResult r)
        {
            r = rtest;
            return r == StageResult.SUCCESS ? true : false;
        }
        #endregion

    }
}
