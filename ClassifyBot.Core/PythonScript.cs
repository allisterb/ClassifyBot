using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

using Python.Runtime;
using Serilog;
using SerilogTimings;

namespace ClassifyBot
{
    public class PythonScript : Extern<PythonScript>
    {
        #region Constructors
        public PythonScript(string binDir = "", string scriptPath = "", bool isModule = false, params string[] argv) : base(binDir, scriptPath, argv)
        {
            if (!scriptPath.IsEmpty())
            {
                Args.Insert(0, ModulePath);
                if (isModule)
                {
                    Args.Insert(1, "-m");
                }
            }
        }
        #endregion

        #region Overriden members
        public override bool Init()
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            if (!BinDir.IsEmpty())
            {
                Directory.SetCurrentDirectory(BinDir);
            }

            Operation init = Operation.Begin("Initializing embedded Python interpreter");
            if (PythonEngine.IsInitialized && (BinDir != string.Empty || ModulePath != string.Empty || Args.Count > 0))
            {
                Error("Python engine is already initialized and cannot be initilaized with another script or aditional arguments.");
                init.Cancel();
                return false;
            }

            try
            {
                PythonEngine.Initialize(Args);
            }
            catch (DllNotFoundException dnfe)
            {
                    
                if (BinDir.IsEmpty())
                {
                    Error(dnfe, $"Could not find the system-wide python36 shared library. Check your PATH environment variable or use the -P option to set the path to a interpreter directory.");
                }
                else
                {
                            
                    Error(dnfe, $"Could not find python36 shared library in {BinDir}.");
                }
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown initalizing Python engine.");
            }
            finally
            {
                if (!BinDir.IsEmpty())
                {
                    Directory.SetCurrentDirectory(originalDirectory);
                }
            }

            if (PythonEngine.IsInitialized)
            {
                PythonEngine.ProgramName = "ClassifyBot";
                Info("Python version {0} from {1}.", PythonEngine.Version.Trim(), BinDir.IsEmpty() ? Runtime.PythonDLL.WithDllExt() : Path.Combine(BinDir, Runtime.PythonDLL.WithDllExt()));
                Modules = GetModules();
                Info("{0} modules installed.", Modules.Count(m => !m.StartsWith("__")));
                HasPipModule = Modules.Contains("pip");
                if (!HasPipModule)
                {
                    Warn("pip modules is not installed. Package operations will fail.");
                }
                else
                {
                    PipModules = GetPipModules();
                    Info("{0} pip modules installed.", PipModules.Count);
                }
                init.Complete();
                return this.Initialized = true;
            }
            else
            {
                Error("Could not initalize Python engine.");
                init.Cancel();
                return false;
            }
        }


        public override bool Run()
        {
            
            throw new NotImplementedException();
        }

        public override bool Destroy()
        {
            PythonEngine.Shutdown();
            return true;
        }
        #endregion

        #region Properties
        public List<string> Modules { get; protected set; }
        public bool HasPipModule { get; protected set; }
        public Dictionary<string, string> PipModules { get; protected set; }
        #endregion

        #region Methods
        protected List<string> GetModules()
        {
            Contract.Requires(this.Initialized);
            List<string> results = new List<string>();
            using (Py.GIL())
            using (PyIter modules = new PyIter(Py.Import("pkgutil").InvokeMethod("iter_modules")))
            {
                while (modules.MoveNext())
                {
                    PyObject m = (PyObject) modules.Current;
                    results.Add(m[1].As<string>());
                }
            }
            return results;
        }
        protected Dictionary<string, string> GetPipModules()
        {
            Contract.Requires(this.Initialized);
            Dictionary<string, string>  results = new Dictionary<string, string>();
            using (Py.GIL())
            using (PyIter modules = new PyIter(Py.Import("pip").InvokeMethod("get_installed_distributions").GetIterator()))
            {
                while (modules.MoveNext())
                {
                    dynamic m = (PyObject)modules.Current;
                    results.Add(m.key.As<string>(), m.version.As<string>());
                }
            }
            return results;
        }
        #endregion
    }
}
