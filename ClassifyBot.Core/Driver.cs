using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using CommandLine;
using CommandLine.Text;
using Figgle;

namespace ClassifyBot
{
    public static class Driver
    {
        #region Constructors
        static Driver()
        {
            LoadedAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("ClassifyBot.*.dll", "ClassifyBot.Base.dll", "ClassifyBot.Cli.dll");
            if (LoadedAssemblies == null)
            {
                throw new Exception("Did not load assembly ClassifyBot.Core.dll");
            }
        }
        #endregion

        #region Properties
        public static List<Assembly> LoadedAssemblies { get; }
        #endregion

        #region Methods
        public static Type[] GetSubTypes<T>()
        {
            return LoadedAssemblies
                 .Select(a => a.GetTypes())
                 .SelectMany(t => t)
                 .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)?.ToArray();
        }

        public static Stage MarshalOptionsForStage(string[] args, out StageResult result, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            result = StageResult.INVALID_OPTIONS;
            Parser p = new Parser();
            ParserResult<object> options = p.ParseArguments(args, GetSubTypes<Stage>());
            string oh = string.Empty;
            StageResult sr = StageResult.INVALID_OPTIONS;
            Stage stage = null;
            options
                .WithNotParsed((errors) =>
                {
                    oh = GetHelpForInvalidOptions(options, errors);
                    sr = StageResult.INVALID_OPTIONS;
                })
                .WithParsed((o) => { stage = (Stage)o; sr = StageResult.INIT; });
            optionsHelp = oh;
            result = sr;
            return stage;
        }

        public static T MarshalOptionsForStage<T>(string[] args, out StageResult result, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            result = StageResult.INVALID_OPTIONS;
            Parser p = new Parser();
            ParserResult<object> options = p.ParseArguments(args, typeof(T));
            string oh = string.Empty;
            StageResult sr = StageResult.INVALID_OPTIONS;
            T stage = default(T);
            options
                .WithNotParsed((errors) =>
                {
                    oh = GetHelpForInvalidOptions(options, errors);
                    sr = StageResult.INVALID_OPTIONS;
                })
                .WithParsed((o) => { stage = (T) o; sr = StageResult.INIT; });
            optionsHelp = oh;
            result = sr;
            return stage;
        }


        public static string GetHelpForInvalidOptions(ParserResult<object> result, IEnumerable<Error> errors)
        {
            Type[] stageTypes = GetSubTypes<Stage>();
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

    }
}
