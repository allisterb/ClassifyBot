using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks;

namespace ClassifyBot.Cli
{
    class Program
    {
        static Dictionary<string, string> AppConfig { get; set; }
        static ILogger L;
       
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.RollingFile(Path.Combine("logs", "ClassifyBot") + "-{Date}.log")
                .CreateLogger();
            L = Log.ForContext<Program>();

            Stage s = Stage.MarshalOptionsForStage(args, out string optionsHelp);
        }

        static void Exit(ExitResult result)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)result);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            Log.Error(exception, "An unhandled exception occurred. The program will now shutdown.");
            //Log.Error(exception.StackTrace);
            Exit(ExitResult.UNHANDLED_RUNTIME_EXCEPTION);
        }
    }
}
