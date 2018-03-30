using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using CommandLine;
using CommandLine.Text;
using Figgle;
using Serilog;

namespace ClassifyBot
{
    public class Driver
    {
        #region Constructors
        static Driver()
        {
            L = Log.ForContext<Driver>();
            AllLoadedAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("ClassifyBot.*.dll", "ClassifyBot.Base.dll", "ClassifyBot.Cli.dll");
            foreach(string n in ExcludedAssemblyNames)
            {
                if (AllLoadedAssemblies.Any(a => a.FullName.StartsWith(n)))
                {
                    AllLoadedAssemblies.RemoveAll(a => a.FullName.StartsWith(n));
                }
            }
            if (AllLoadedAssemblies == null)
            {
                throw new Exception("Did not load assembly ClassifyBot.Core.dll");
            }
        }
        #endregion

        #region Properties
        public static List<Assembly> AllLoadedAssemblies { get; internal set; }
        public static List<Assembly> ClassifyBotLoadedAssemblies { get; internal set; }
        public static string[] ExcludedAssemblyNames { get; } = new string[] { "ClassifyBot.Core", "ClassifyBot.Classifier" };
        #endregion

        #region Methods
        public static Type[] GetSubTypes<T>(string assemblyName = "")
        {
            IEnumerable<Assembly> assemblies = AllLoadedAssemblies;
            if (AllLoadedAssemblies.Count(a => assemblyName.IsNotEmpty() && a.GetName().Name == assemblyName) > 0)
            {
                assemblies = AllLoadedAssemblies.Where(a => a.FullName.StartsWith(assemblyName));
            }
            else if (assemblyName.IsNotEmpty())
            {
                return null;
            }
           
            return assemblies
                 .Select(a => a.GetTypes())
                 .SelectMany(t => t)
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)?
                 .ToArray();
        }

        public static StageResult MarshalOptionsForStage(string[] args, string explicitAssemblyName, out Stage stage, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            Stage s = null;
            Parser p = new Parser();
            Type[] types = GetSubTypes<Stage>(explicitAssemblyName);
            if ((types == null || types.Length == 0) && explicitAssemblyName.IsNotEmpty())
            {
                stage = null;
                L.Error("No assemblies matching the name {0}.dll in directory {1} were found.".F(explicitAssemblyName, AssemblyExtensions.GetExecutingAssemblyDirectoryName()));
                optionsHelp = "You must enter an assembly name to explicitly load. Valid assembly names are : {0}."
                            .F(string.Join(", ", AllLoadedAssemblies.Select(a => a.GetName().Name).ToArray()));
                return StageResult.INVALID_OPTIONS;
            }
            else
            {
                ClassifyBotLoadedAssemblies = types.Select(t => t.Assembly).Distinct().ToList();
                if (ClassifyBotLoadedAssemblies.Count == 1)
                {
                    L.Information("Loaded 1 ClassifyBot assembly: {1}.", ClassifyBotLoadedAssemblies.Count(), ClassifyBotLoadedAssemblies.Select(a => a.FullName));
                }
                else
                {
                    L.Information("Loaded {0} ClassifyBot assemblies: {1}.", ClassifyBotLoadedAssemblies.Count(), ClassifyBotLoadedAssemblies.Select(a => a.FullName));
                }

            }
            ParserResult<object> options = p.ParseArguments(args, types);
            string oh = string.Empty;
            StageResult sr = StageResult.INVALID_OPTIONS;
            options
                .WithNotParsed((errors) =>
                {
                    oh = GetHelpForInvalidOptions(options, errors);
                    sr = StageResult.INVALID_OPTIONS;
                })
                .WithParsed((o) => { s = (Stage) o; sr = StageResult.CREATED; });
            optionsHelp = oh;
            stage = s;
            return sr;
        }

        public static StageResult MarshalOptionsForStage<T>(string[] args, out T stage, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            T s = default(T);
            Parser p = new Parser();
            ParserResult<object> options = p.ParseArguments(args, typeof(T));
            string oh = string.Empty;
            StageResult sr = StageResult.INVALID_OPTIONS;
            options
                .WithNotParsed((errors) =>
                {
                    oh = GetHelpForInvalidOptions(options, errors);
                    sr = StageResult.INVALID_OPTIONS;
                })
                .WithParsed((o) => { s = (T) o; sr = StageResult.CREATED; });
            optionsHelp = oh;
            stage = s;
            return sr;
        }


        public static string GetHelpForInvalidOptions(ParserResult<object> result, IEnumerable<Error> errors)
        {
            Type[] stageTypes = GetSubTypes<Stage>(ExplicitAssemblyName);
            HelpText help = HelpText.AutoBuild(result, h =>
            {
                h.AddOptions(result);
                return h;
            },
            e =>
            {
                return e;
            },
            true);
            help.MaximumDisplayWidth = Console.WindowWidth;
            help.Copyright = string.Empty;
            help.Heading = new HeadingInfo("ClassifyBot", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            help.AddPreOptionsLine(string.Empty);

            if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
            {
                help.Heading = string.Empty;
                return "{2}{0}v{1}".F(FiggleFonts.Chunky.Render("ClassifyBot"), Assembly.GetExecutingAssembly().GetName().Version.ToString(4), Environment.NewLine);
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpVerbRequestedError))
            {
                HelpVerbRequestedError error = (HelpVerbRequestedError)errors.First(e => e.Tag == ErrorType.HelpVerbRequestedError);
                if (error.Type != null)
                {
                    help.AddVerbs(error.Type);
                }
                else
                {
                    help.AddVerbs(stageTypes);
                }

                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpRequestedError))
            {
                HelpRequestedError error = (HelpRequestedError)errors.First(e => e.Tag == ErrorType.HelpRequestedError);
                help.AddOptions(result);
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
            {
                help.AddVerbs(stageTypes);
                help.AddPreOptionsLine("No stage selected. Select a stage or verb from the ones listed below:");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.BadVerbSelectedError))
            {
                BadVerbSelectedError error = (BadVerbSelectedError)errors.First(e => e.Tag == ErrorType.BadVerbSelectedError);
                help.AddVerbs(stageTypes);
                help.AddPreOptionsLine($"Unknown stage: {error.Token}. Valid stages and verbs are:");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.MissingRequiredOptionError))
            {
                MissingRequiredOptionError error = (MissingRequiredOptionError)errors.First(e => e is MissingRequiredOptionError);
                help.AddOptions(result);
                help.AddPreOptionsLine($"A required option or value is missing. The options and values for this stage are: ");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.MissingValueOptionError))
            {
                MissingValueOptionError error = (MissingValueOptionError)errors.First(e => e.Tag == ErrorType.MissingValueOptionError);
                help.AddOptions(result);
                help.AddPreOptionsLine($"A required option or value is missing. The options and values for this stage are: ");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.UnknownOptionError))
            {
                UnknownOptionError error = (UnknownOptionError)errors.First(e => e.Tag == ErrorType.UnknownOptionError);
                help.AddOptions(result);
                help.AddPreOptionsLine($"Unknown option: {error.Token}.");
                return help;
            }
            else
            {
                help.AddPreOptionsLine($"An error occurred parsing the program options: {string.Join(" ", errors.Select(e => e.Tag.ToString()).ToArray())}");
                help.AddVerbs(stageTypes);
                return help;
            }
        }
        #endregion

        #region Fields
        protected static ILogger L;
        protected static string ExplicitAssemblyName;
        #endregion

    }
}
