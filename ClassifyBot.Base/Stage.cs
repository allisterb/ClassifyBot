using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


using CommandLine;
using Serilog;
namespace ClassifyBot
{
    public abstract class Stage
    {
        #region Constructors
        public Stage()
        {
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                if (!StageOptions.ContainsKey(prop.Name))
                {
                    StageOptions.Add(prop.Name, prop.GetValue(this));
                }
            }
        }
        #endregion

        #region Properties
        public virtual ILogger L { get; } = Log.ForContext<Stage>();
        
        public static Dictionary<string, object> StageOptions { get; } = new Dictionary<string, object>();

        public bool Initialized { get; protected set; } = false;

        [Option("debug", HelpText = "Enable debug output.", Required = false)]
        public bool DebugOutput { get; set; }

        [Option("verbose", HelpText = "Enable verbose output.", Required = false)]
        public bool VerboseOutput { get; set; }

        //[Option("explicit", HelpText = "Enable explicit loading of assemblies.", Required = false)]
        public string ExplicitAssemblies { get; set; }
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
        #endregion

    }
}
