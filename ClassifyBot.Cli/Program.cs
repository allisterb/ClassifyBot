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
        static Dictionary<string, object> PipelineOptions { get; set; }
        static ILogger L;
       
        static FileInfo InputFile { get; set; }
        static FileInfo ExtractOutputFile { get; set; }
        static FileInfo TrainingOutputFile { get; set; }
        static FileInfo TestOutputFile { get; set; }
        static FileInfo TargetOutputFile { get; set; }

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.RollingFile(Path.Combine("logs", "ClassifyBot") + "-{Date}.log")
                .CreateLogger();
            L = Log.ForContext<Program>();
            CommandLineOptions.MarshalExtractorOptions(args);
        }
    }
}
