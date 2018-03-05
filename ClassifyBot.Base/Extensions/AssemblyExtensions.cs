using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public static class AssemblyExtensions
    {
        private static ILogger L = Log.ForContext<Assembly>();
        public static List<Assembly> LoadAllFrom(this Assembly assembly, string[] includedFileNames, params string[] excludedFileNames)
        {
            if (includedFileNames == null)
            {
                L.Debug("No included files passed to LoadAllFrom() method");
                return null;
            }
            List<Assembly> assemblies = new List<Assembly>();
            bool hasExlusions = excludedFileNames != null;
            for (int i = 0; i < includedFileNames.Length; i++)
            {
                if (hasExlusions && excludedFileNames.Any(f => includedFileNames[i].EndsWith(f)))
                {
                    continue;
                }
                else if (!File.Exists(includedFileNames[i]))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        assemblies.Add(Assembly.LoadFrom(includedFileNames[i]));
                    }
                    catch (Exception e)
                    {
                        L.Error(e, "Exception thrown loading assembly from file {0}.", includedFileNames[i]);
                    }

                }
            }
            if (assemblies.Count == 0)
            {
                L.Debug("No external assemblies matching search pattern loaded.");
                return null;
            }
            else
            {
                L.Information("Loaded {0} external assemblies: {1}.", assemblies.Count, assemblies);
                return assemblies;

            }
        }

        public static List<Assembly> LoadAllFrom(this Assembly assembly, string includedFilePattern, params string[] excludedFileNames)
        {
            string[] assemblyFiles = null;
            try
            {
                assemblyFiles = Directory.GetFiles(GetExecutingDirectoryName(), "ClassifyBot.*.dll").ToArray();
            }
            catch (Exception e)
            {
                L.Error(e, "Exception thrown searching directory {0} for file pattern {1}.", Directory.GetCurrentDirectory(), includedFilePattern);
                return null;
            }
            if (assemblyFiles == null)
            {
                L.Debug("No included files passed to LoadAllFrom() method");
                return null;
            }
            else
            {
                return LoadAllFrom(assembly, assemblyFiles, excludedFileNames);
            }
        }

        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory.FullName;
        }
    }
}
