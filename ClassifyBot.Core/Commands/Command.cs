using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using MeSh = Medallion.Shell;
using Serilog;

namespace ClassifyBot
{
    public class Command
    {
        #region Constuctors
        public Command(string workingDirectory, string cmdText, params object[] cmdOptions)
        {
            if (cmdText.Empty())
            {
                throw new ArgumentException("The cmdText parameter must not be null and empty.");
            }
            this.WorkingDirectory = workingDirectory;
            CommandText = cmdText;
            CommandOptions = cmdOptions.ToList();
            shell = new MeSh.Shell(o => o.WorkingDirectory(WorkingDirectory));
        }
        #endregion

        #region Properties
        public string CommandText { get; protected set; }
        public string WorkingDirectory { get; protected set; }
        public List<object> CommandOptions { get; protected set; }
        public Task CommandTask { get; protected set; }
        public bool WorkingDirectoryExists => !WorkingDirectory.Empty() ? Directory.Exists(WorkingDirectory) : false;

        public bool CommandStarted => meshCommand != null;

        public Exception Exception { get; protected set; }
        
        public bool Success
        {
            get
            {
                if (_Success.HasValue)
                {
                    return _Success.Value;
                }
                else if (CommandTask.IsCompleted)
                {
                    _Success = CommandResult.Success;
                    return _Success.Value;
                }
                else
                {
                    return false;
                }
            }
        }

        public string StandardOutput => CommandResult?.StandardOutput;

        public string StandardError => CommandResult?.StandardError;

        protected MeSh.CommandResult CommandResult
        {
            get
            {
                if (!CommandStarted)
                {
                    throw new InvalidOperationException("The command has not started.");
                }
                else if (CommandTask.IsCanceled || CommandTask.IsFaulted)
                {
                    return null;
                }
                else if (commandResult != null )
                {
                    return commandResult;
                }
                else
                {
                    try
                    {
                        commandResult = meshCommand.Result;
                    }
                    catch (Exception e)
                    {
                        Exception = e;
                        L.Error(e, "Exception thrown running command {0}.", CommandText);
                    }
                    return commandResult;
                }
            }
        }
        #endregion

        #region Methods
        public virtual Task Run()
        {
            try
            {
                meshCommand = shell.Run(CommandText, CommandOptions.ToArray());
                return CommandTask = meshCommand.Task;
            }
            catch (Exception e)
            {
                Exception = e;
                L.Error(e, "An exception was thrown attempting to execute command {0}.", CommandText);
                _Success = false;
                return CommandTask = Task.FromException(e);
            }
            
        }
        #endregion

        #region Fields
        protected MeSh.Command meshCommand;
        protected MeSh.Shell shell;
        protected MeSh.CommandResult commandResult;
        protected bool? _Success;
        protected static ILogger L = Log.ForContext<Command>();
        #endregion
    }
}
