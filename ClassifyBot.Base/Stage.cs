using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CommandLine;
using CommandLine.Text;

namespace ClassifyBot
{
    public abstract class Stage
    {
        #region Constructors
        static Stage()
        {
            LoadedAssemblies = Assembly.GetExecutingAssembly().LoadAllFrom("ClassifyBot.*.dll", "ClassifyBot.Base.dll", "ClassifyBot.Cli.dll");
            if (LoadedAssemblies == null)
            {
                throw new Exception("Did not load assembly ClassifyBot.Core.dll");
            }
        }

        public Stage()
        {
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                if (!StageOptions.ContainsKey(prop.Name))
                {
                    StageOptions.Add(prop.Name, prop.GetValue(this));
                }
            }
        }
        #endregion

        #region Properties
        public static List<Assembly> LoadedAssemblies { get; }
        public static Dictionary<string, object> StageOptions { get; } = new Dictionary<string, object>();
        #endregion

        #region Methods
        public static Type[] GetSubTypes<T>()
        {
            return LoadedAssemblies
                 .Select(a => a.GetTypes())
                 .SelectMany(t => t)
                 .Where(t => t.IsSubclassOf(typeof(T)) && ! t.IsAbstract)?.ToArray();
        }

        public static Stage MarshalOptionsForStage(string[] args, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            Parser p = new Parser();
            ParserResult<object> result = p.ParseArguments(args, GetSubTypes<Stage>());
            string helpText = string.Empty;
            Stage stage = null;
            result
                .WithNotParsed((errors) => helpText = GetHelpForInvalidOptions(result, errors))
                .WithParsed((o) => stage = (Stage) o);
            optionsHelp = helpText;
            return stage;
        }

        public static T MarshalOptionsForStage<T>(string[] args, out string optionsHelp)
        {
            optionsHelp = string.Empty;
            Parser p = new Parser();
            ParserResult<object> result = p.ParseArguments(args, typeof(T));
            string helpText = string.Empty;
            T stage = default(T);
            result
                .WithNotParsed((errors) => helpText = GetHelpForInvalidOptions(result, errors))
                .WithParsed((o) => stage = (T) o);
            optionsHelp = helpText;
            return stage;
        }


        public static string GetHelpForInvalidOptions(ParserResult<object> result, IEnumerable<Error> errors)
        {
            Type[] ClassifierOptionsTypes = GetSubTypes<Stage>();
            HelpText help = HelpText.AutoBuild(result, h =>
            {
                h.AddOptions(result);
                return h;
            },
            e =>
            {
                return e;
            });
            help.MaximumDisplayWidth = Console.WindowWidth;
            help.Copyright = string.Empty;
            help.Heading = new HeadingInfo("ClassifyBot", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            help.AddPreOptionsLine(string.Empty);
            if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
            {
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpVerbRequestedError))
            {
                HelpVerbRequestedError error = (HelpVerbRequestedError)errors.First(e => e.Tag == ErrorType.HelpVerbRequestedError);
                if (error.Type != null)
                {
                    help.AddVerbs(error.Type);
                }
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.HelpRequestedError))
            {
                help.AddVerbs(ClassifierOptionsTypes);
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
            {
                help.AddVerbs(ClassifierOptionsTypes);
                help.AddPreOptionsLine("No category selected. Select a category from the options below:");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.MissingRequiredOptionError))
            {
                MissingRequiredOptionError error = (MissingRequiredOptionError)errors.First(e => e is MissingRequiredOptionError);
                help.AddOptions(result);
                help.AddPreOptionsLine($"A required option or value is missing. The options and values for this benchmark category are: ");
                return help;
            }
            else if (errors.Any(e => e.Tag == ErrorType.MissingValueOptionError))
            {
                MissingValueOptionError error = (MissingValueOptionError)errors.First(e => e.Tag == ErrorType.MissingValueOptionError);
                help.AddOptions(result);
                help.AddPreOptionsLine($"A required option or value is missing. The options and values for this benchmark category are: ");
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
                help.AddVerbs(ClassifierOptionsTypes);
                return help;
            }
        }
        #endregion
    }
}
