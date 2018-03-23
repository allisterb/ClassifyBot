using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Python.Runtime;
namespace ClassifyBot
{
    public class PythonScript : Extern
    {
        public PythonScript(string binDir, string scriptPath, bool isModule = false, params string[] argv) : base(binDir, scriptPath, argv)
        {
            Contract.Requires(Directory.Exists(binDir));
            Contract.Requires(File.Exists(scriptPath));
            
            Args.Insert(0, ModulePath);
            if (isModule)
            {
                Args.Insert(1, "-m");
            }
        }

        public override bool IsInitialized => PythonEngine.IsInitialized;

        public override bool Init()
        {
            Directory.SetCurrentDirectory(BinDir);
            PythonEngine.Initialize(Args);
            return PythonEngine.IsInitialized;
        }

        public override bool Run()
        {
            throw new NotImplementedException();
        }

        public override bool Destroy()
        {
            throw new NotImplementedException();
        }

        //public 
    }
}
