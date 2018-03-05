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
        static void Main(string[] args) => ConsoleDriver.RunAndExit(args); 
    }
}
