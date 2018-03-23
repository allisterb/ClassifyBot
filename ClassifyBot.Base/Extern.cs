using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassifyBot
{
    public abstract class Extern
    {
        #region Constructors
        public Extern(string binDir, string modulePath, params string[] argv)
        {
            BinDir = binDir;
            ModulePath = modulePath;
            Args = argv?.ToList();
        }
        #endregion

        #region Properties
        public string BinDir { get; }
        public string ModulePath { get; protected set; }
        public List<string> Args { get; }
        #endregion

        #region Abstract members
        public abstract bool IsInitialized { get; } 
        public abstract bool Init();
        public abstract bool Run();
        public abstract bool Destroy();
        #endregion
    }
}
