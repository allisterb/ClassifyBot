using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Serilog;
using Serilog.Enrichers;
using Serilog.Sinks;

namespace ClassifyBot
{
    public class ConsoleDriver : Driver
    {
        static ConsoleDriver()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        #region Properties
        static Dictionary<string, string> AppConfig { get; set; }
        static LoggerConfiguration LoggerConfiguration { get; set; }
        static bool ExitToEnvironment { get; set; }
        public static bool WithLogFile { get; set; } = false;
        public static string LogFileName { get; set; }
        public static bool WithoutConsole { get; set; } = false;
        public static bool WithDebugOutput { get; set; } = false;
        
        #endregion

        #region Methods
        public static void RunAndExit(string[] args)
        {
            if (args.Contains("--wait-for-attach"))
            {
                Console.WriteLine("Attach debugger and press any key to continue execution...");
                Console.ReadKey(true);
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("No debugger detected! Exiting.");
                    return;
                }
                else
                {
                    Debugger.Break();
                }
            }
            if (args.Contains("--with-debug"))
            {
                WithDebugOutput = true;
            }
            if (args.Contains("--with-log-file"))
            {
                WithLogFile = true;
            }
            if (args.Contains("--without-console"))
            {
                WithoutConsole = true;
            }
            
            LoggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId();
            if (WithDebugOutput)
            {
                LoggerConfiguration = LoggerConfiguration.MinimumLevel.Debug();
            }
            if (!WithoutConsole)
            {
                LoggerConfiguration = LoggerConfiguration.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{ThreadId:d2}][{Level:u3}] {Message}{NewLine}{Exception}");
            }
            if (WithLogFile)
            {
                LogFileName = Path.Combine("logs", "ClassifyBot") + "-{Date}.log";
                LoggerConfiguration = LoggerConfiguration.WriteTo.RollingFile(LogFileName, outputTemplate: "{Timestamp:HH:mm:ss}[{ThreadId:d2}] [{Level:u3}] {Message}{NewLine}{Exception}");
            }

            Log.Logger = LoggerConfiguration.CreateLogger();
            L = Log.ForContext<Driver>();

            ExitToEnvironment = true;

            if (WithLogFile)
            {
                L.Information("Log file is at {0}.", LogFileName);
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-x" || args[i] == "--explicit")
                {
                    if ((i + 1) <= args.Length - 1)
                    {
                        ExplicitAssemblyName = args[i + 1].StartsWith("ClassifyBot.") ? args[i + 1] : "ClassifyBot." + args[i + 1];
                        args = args.Except(new string[] { "-e", "--explicit", args[i + 1] }).ToArray();
                    }
                    else
                    {
                        L.Error("You must enter an assembly name to explicitly load. Valid assembly names are : {0}."
                            .F(string.Join(", ", AllLoadedAssemblies.Select(a => a.GetName().Name).ToArray())));
                        Exit(StageResult.INVALID_OPTIONS);
                    }
                    break;
                }
            }

            StageResult result = MarshalOptionsForStage(args, ExplicitAssemblyName, out Stage stage, out string optionsHelp);
            if (result == StageResult.INVALID_OPTIONS && stage == null && !optionsHelp.IsEmpty())
            {
                L.Information(optionsHelp);
            }
            else if (result == StageResult.CREATED && stage != null && optionsHelp.IsEmpty())
            {
                Exit(stage.Run());
            }
            else
            {
                throw new Exception("Unknown stage state {0} {1}.".F( result, stage));
            }
        }

        static void Exit(StageResult result)
        {
            Log.CloseAndFlush();
            ExitResult er = ExitResult.STAGE_FAILED;
            if (result == StageResult.SUCCESS)
            {
                er = ExitResult.SUCCESS;
            }
            else
            {
                er = ExitResult.STAGE_FAILED;
            }
            Environment.Exit((int) er);
        }

        static void Exit(ExitResult result)
        {
            Log.CloseAndFlush();
            Environment.Exit((int)result);
        }
        #endregion

        #region Event handlers
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            Log.Error(exception, "An unhandled runtime exception occurred.");
            if (ExitToEnvironment)
            {
                Exit(ExitResult.UNHANDLED_RUNTIME_EXCEPTION);
            }
        }
        #endregion
    }
}
