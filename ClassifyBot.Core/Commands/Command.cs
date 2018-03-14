using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using MeSh = Medallion.Shell;
using Serilog;

namespace ClassifyBot
{
    public class Command
    {
        #region Constuctors
        public Command(string cmdText, params object[] cmdOptions)
        {
            if (cmdText.Empty())
            {
                throw new ArgumentException("The cmdText parameter must not be null and empty.");
            }
            CommandText = cmdText;
            CommandOptions = cmdOptions;
            shell = new MeSh.Shell(o => {});
        }

        public Command(string workingDirectory, string cmdText, params object[] cmdOptions) : this(cmdText, cmdOptions)
        {
            this.WorkingDirectory = workingDirectory;
            shell = new MeSh.Shell(o => o.WorkingDirectory(WorkingDirectory));
        }
        #endregion

        #region Properties
        public string CommandText { get; protected set; }
        public virtual string WorkingDirectory { get; protected set; }
        public object[] CommandOptions { get; protected set; }
        public Task CommandTask { get; protected set; }
        public bool WorkingDirectoryExists => !WorkingDirectory.Empty() ? Directory.Exists(WorkingDirectory) : false;
        #endregion

        #region Methods
        public virtual Task Run()
        {
            
            meshCommand = shell.Run(CommandText, CommandOptions);
            return CommandTask = meshCommand.Task;
        }
        #endregion

        #region Fields
        protected MeSh.Command meshCommand;
        protected MeSh.Shell shell;
        #endregion


    }
}
