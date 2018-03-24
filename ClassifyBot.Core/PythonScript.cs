using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Python.Runtime;
using Serilog;
using SerilogTimings;

namespace ClassifyBot
{
    public class PythonScript : Extern<PythonScript>
    {
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

        public override bool IsInitialized => PythonEngine.IsInitialized;

        public override bool Init()
        {
            string originalDirectory = Directory.GetCurrentDirectory();
            if (!BinDir.IsEmpty())
            {
                Directory.SetCurrentDirectory(BinDir);
            }
            using (Operation init = Operation.Begin("Initializing embedded Python interpreter")) 
            {
                try
                {
                    PythonEngine.Initialize(Args);
                }
                catch (DllNotFoundException dnfe)
                {
                    
                    if (BinDir.IsEmpty())
                    {
                        Error(dnfe, $"Could not find the system-wide {"python36".WithSharedLibraryExtension()} library. Check your PATH environment variable or use the -P option to set the path to the interpreter directory.");
                    }
                    else
                    {
                            
                        Error(dnfe, $"Could not find {"python36".WithSharedLibraryExtension()} in {BinDir}.");
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
                    Info("Python version {0} from {1}.", PythonEngine.Version, BinDir.IsEmpty() ? Runtime.PythonDLL.WithSharedLibraryExtension() : Path.Combine(BinDir, Runtime.PythonDLL.WithSharedLibraryExtension()));
                    init.Complete();
                }
                else init.Cancel();
            }
            return PythonEngine.IsInitialized;
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
    }
}
