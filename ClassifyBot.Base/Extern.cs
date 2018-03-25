using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public abstract class Extern<T>
    {
        #region Constructors
        public Extern(string binDir = "", string modulePath = "", params string[] argv)
        {
            Contract.Requires(binDir.IsEmpty() || Directory.Exists(binDir));
            Contract.Requires(modulePath.IsEmpty() || File.Exists(modulePath));
            BinDir = binDir;
            ModulePath = modulePath;
            Args = argv?.ToList();
        }
        #endregion

        #region Properties
        public string BinDir { get; protected set; }
        public string ModulePath { get; protected set; }
        public List<string> Args { get; protected set; }
        public bool Initialized { get; protected set; }
        #endregion

        #region Methods
        [DebuggerStepThrough] protected virtual void Info(string messageTemplate, params object[] propertyValues) => L.Information(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Debug(string messageTemplate, params object[] propertyValues) => L.Debug(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(string messageTemplate, params object[] propertyValues) => L.Error(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(Exception e, string messageTemplate, params object[] propertyValues) => L.Error(e, messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Verbose(string messageTemplate, params object[] propertyValues) => L.Verbose(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Warn(string messageTemplate, params object[] propertyValues) => L.Warning(messageTemplate, propertyValues);
        #endregion

        #region Abstract members
        
        public abstract bool Init();
        public abstract bool Run();
        public abstract bool Destroy();
        #endregion

        #region Fields
        protected static ILogger L = Log.ForContext<T>();
        #endregion
    }
}
