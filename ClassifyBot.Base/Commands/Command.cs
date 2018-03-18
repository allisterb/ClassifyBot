using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using ExpectNet;
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
            Text = cmdText;
            CommandOptions = cmdOptions.ToList();
            shell = new MeSh.Shell(o => o.WorkingDirectory(WorkingDirectory));
        }
        #endregion

        #region Properties
        public string Text { get; protected set; }
        public string WorkingDirectory { get; protected set; }
        public List<object> CommandOptions { get; protected set; }
        public Task Task { get; protected set; }
        public bool WorkingDirectoryExists => !WorkingDirectory.Empty() ? Directory.Exists(WorkingDirectory) : false;

        public bool Started => meshCommand != null;

        public Exception Exception { get; protected set; }
        
        public bool Success
        {
            get
            {
                if (_Success.HasValue)
                {
                    return _Success.Value;
                }
                else if (Task.IsCompleted)
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

        public string OutputText => CommandResult?.StandardOutput;

        public string ErrorText => CommandResult?.StandardError;

        protected MeSh.CommandResult CommandResult
        {
            get
            {
                if (!Started)
                {
                    throw new InvalidOperationException("The command has not started.");
                }
                else if (Task.IsCanceled || Task.IsFaulted)
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
                        L.Error(e, "Exception thrown running command {0}.", Text);
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
                meshCommand = shell.Run(Text, CommandOptions.ToArray());
                return Task = meshCommand.Task;
            }
            catch (Exception e)
            {
                Exception = e;
                L.Error(e, "An exception was thrown attempting to execute command {0}.", Text);
                _Success = false;
                return Task = Task.FromException(e);
            }    
        }

        public IEnumerable<string> GetOutputAndErrorLines()
        {
            if (!Started)
            {
                throw new InvalidOperationException("The command has not started.");
            }
            else
            {
                return meshCommand?.GetOutputAndErrorLines();
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
