using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public PythonScript(string homeDir = "", string scriptPath = "", bool isModule = false, params string[] argv) : base(homeDir, scriptPath, argv)
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
            Operation init = Operation.Begin("Initializing embedded Python interpreter");
            if (PythonEngine.IsInitialized && (HomeDir != string.Empty || ModulePath != string.Empty || Args.Count > 0))
            {
                Error("Python engine is already initialized and cannot be initilaized with another script or aditional arguments.");
                init.Cancel();
                return false;
            }
            
            
            string originalDirectory = Directory.GetCurrentDirectory();

            try
            {
                SetVirtualEnvDir();
                SetBinDir();
                if (binDir.IsNotEmpty())
                {
                    
                    Directory.SetCurrentDirectory(binDir);
                }
                SetPythonPath();
                PythonEngine.Initialize(Args);
            }
            catch (DllNotFoundException dnfe)
            {
                if (HomeDir.IsEmpty())
                {
                    Error(dnfe, $"Could not find the system-wide python36 shared library. Add Python 3.6 to your PATH environment variable or use the -P option to set the path to a Python 3.6 interpreter directory.");
                }
                else
                {
                            
                    Error(dnfe, $"Could not find python36 shared library in {HomeDir}.");
                }
            }
            catch (Exception e)
            {
                Error(e, "Exception thrown initalizing Python engine.");
            }
            finally
            {
                if (Directory.GetCurrentDirectory() != originalDirectory)
                {
                    Directory.SetCurrentDirectory(originalDirectory);
                }
            }

            if (PythonEngine.IsInitialized)
            {
                
                Info("Python version {0} from {1}.", PythonEngine.Version.Trim(), binDir.IsEmpty() ? Runtime.PythonDLL : Path.Combine(binDir, Runtime.PythonDLL.WithDllExt()));
                Modules = GetModules();
                Info("{0} modules installed.", Modules.Count(m => !m.StartsWith("__")));
                HasPipModule = Modules.Contains("pip");
                if (!HasPipModule)
                {
                    Warn("pip module is not installed.");
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
        public bool VirtualEnvActivated { get; protected set; }
        public DirectoryInfo VirtualEnvDir { get; protected set; }
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

        protected void SetVirtualEnvDir()
        {
            string venv = Environment.GetEnvironmentVariable("VIRTUAL_ENV");
            if (!venv.IsEmpty() && Directory.Exists(venv))
            {
                VirtualEnvActivated = true;
                VirtualEnvDir = new DirectoryInfo(venv);
                Info("Virtual environment activation detected.");
            }
            else if (homeDirInfo != null && homeDirInfo.GetFiles("pyvenv.cfg").Count() > 0)
            {
                VirtualEnvDir = homeDirInfo;
                Info("Virtual environment directory detected.");
            }
        }

        protected void SetBinDir()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && !VirtualEnvActivated && VirtualEnvDir != null && VirtualEnvDir.GetDirectories().Any(d => d.Name.ToLower() == "scripts"))
            {
                binDir = VirtualEnvDir.GetDirectories().First(d => d.Name.ToLower() == "scripts").FullName;
            }
        }

        protected void SetPythonPath()
        {
            string ppath = Environment.GetEnvironmentVariable("PYTHONPATH");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && VirtualEnvDir != null && ppath.IsEmpty() && VirtualEnvDir.GetDirectories().Any(d => d.Name.ToLower() == "lib"))
            {
                DirectoryInfo lib = VirtualEnvDir.GetDirectories().First(d => d.Name.ToLower() == "lib");
                if (lib.GetDirectories().Any(d => d.Name.ToLower() == "site-packages"))
                {
                    string site_packages = lib.GetDirectories().First(d => d.Name.ToLower() == "site-packages").FullName;
                    Info("Incuding virtual environment user modules directory {0}.", site_packages);
                    PythonEngine.PythonPath += ";{0}".F(site_packages);
                }
            }
            Info("User module paths are: {0}.", PythonEngine.PythonPath);
        }
        #endregion

        #region Fields
        protected string binDir;
        #endregion
    }
}
