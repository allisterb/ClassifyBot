using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using CommandLine;

namespace ClassifyBot
{
    public abstract class StanfordNLPClassifier<TRecord, TFeature> : Classifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        public override StageResult Train(Dictionary<string, string> options = null)
        {
            javaCommand = new JavaCommand(JavaHome, ClassPath, "edu.stanford.nlp.classify.ColumnDataClassifier", "-mx16000m", 
                "-trainFile", TrainingFile.FullName, "-testFile", TestFile.FullName, "-prop", @"C:\Projects\ClassifyBot\data\mood.prop");
            Task c = javaCommand.Run();
            if (!CheckCommandStartedAndReport(javaCommand))
            {
                return StageResult.FAILED;
            }
 

            ClassifierOutput = new List<string>();
            foreach (string s in javaCommand.GetOutputAndErrorLines())
            {
                if (s.StartsWith("Built this classifier"))
                {
                    Match m = builtClassifierRegex.Match(s);
                    if (m.Success)
                    {
                        ClassifierType = m.Groups[1].Value;
                        NumberofFeatures = Int32.Parse(m.Groups[2].Value);
                        NumberofClasses = Int32.Parse(m.Groups[3].Value);
                        NumberofParameters = Int32.Parse(m.Groups[4].Value);
                        Info("Built classifier {0} with {1} features, {2} classes and {3} parameters.", ClassifierType, NumberofFeatures, NumberofClasses, NumberofParameters);
                    }
                    else
                    {
                        Error("Could not parse classifier output line: {0}.", s);
                        return StageResult.FAILED;
                    }
                }
                if (s.StartsWith("Reading dataset from {0} ... done".F(TrainingFile.FullName)))
                {
                    ReadTrainingDataset = true;
                    Match m = readDataSetRegex.Match(s);
                    if (m.Success)
                    {
                        TrainingDataSetItems = Int32.Parse(m.Groups[3].Value);
                        Info("{0} items in training dataset read in {1} s.", TrainingDataSetItems, m.Groups[2].Value);
                    }
                    
                }
                if (s.StartsWith("Reading dataset from {0} ... done".F(TestFile.FullName)))
                {
                    ReadTestDataset = true;
                    Match m = readDataSetRegex.Match(s);
                    if (m.Success)
                    {
                        TestDataSetItems = Int32.Parse(m.Groups[3].Value);
                        Info("{0} items in test dataset read in {1} s.", TestDataSetItems, m.Groups[2].Value);
                    }

                }
                ClassifierOutput.Add(s);
            }
            c.Wait();
            if (!CheckCommandSuccessAndReport(javaCommand))
            {
                return StageResult.FAILED;
            }

            return StageResult.SUCCESS;
        }
        
        protected override StageResult Init()
        {
            StageResult r = base.Init();
            if (r != StageResult.SUCCESS)
            {
                return r;
            }

            if (TrainOp)
            {
                Contract.Requires(ModelFile != null);
            }

            if (JavaHome.Empty())
            {
                if (AdditionalOptions.ContainsKey("JAVA_HOME"))
                {
                    JavaHome = (string)AdditionalOptions["JAVA_HOME"];
                }
                else if ((JavaHome = Environment.GetEnvironmentVariable("JAVA_HOME")).Empty())
                {
                    Error("The java-home option was not specified and the JAVA_HOME environment variable does not exist.");
                    return StageResult.INVALID_OPTIONS;
                }
            }
            if (!Directory.Exists(JavaHome))
            {
                Error("The Java home directory specified does not exist: {0}.", JavaHome);
                return StageResult.INVALID_OPTIONS;
            }

            if (ClassPath.Empty())
            {
                if (AdditionalOptions.ContainsKey("STANFORD_CLASSIFIER_JAR"))
                {
                    ClassPath = (string)AdditionalOptions["STANFORD_CLASSIFIER_JAR"];
                }
                else if ((ClassPath = Environment.GetEnvironmentVariable("STANFORD_CLASSIFIER_JAR")).Empty())
                {
                    Error("The class-path option was not specified and the STANFORD_CLASSIFIER_JAR environment variable does not exist.");
                    return StageResult.INVALID_OPTIONS;
                }
            }
            if (!File.Exists(ClassPath))
            {
                Error("The .jar archive path specified does not exist: {0}.", ClassPath);
                return StageResult.INVALID_OPTIONS;
            }

            Command version = new Command(Path.Combine(JavaHome, "bin"), "java", "-version");
            Task c = version.Run();
            if (!version.Started)
            {
                Error("Could not detect Java version.");
                return StageResult.FAILED;
            }
            else
            {
                c.Wait();
                if (c.IsCompleted && version.Success)
                {
                    Info(version.ErrorText.Replace(Environment.NewLine, " "));
                }
                if (c.IsCompleted && !version.Success)
                {
                    Error("Could not detect Java version: {0}", version.OutputText);
                    return StageResult.FAILED;
                }
            }
            return StageResult.SUCCESS;

        }

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }

        protected override StageResult Save()
        {
            return
                 StageResult.SUCCESS;
        }
        #endregion

        #region Properties
        [Option('J', "java-home", Required = false, HelpText = "The path to an existing Java installation. If this is not specified then the JAVA_HOME environment variable will be used")]
        public virtual string JavaHome { get; set; }

        [Option('C', "class-path", Required = false, HelpText = "The path to the Stanford NLP Classifier jar file. If this is not specified then the JAVA_HOME environment variable will be used")]
        public virtual string ClassPath { get; set; }

        public List<string> ClassifierOutput { get; protected set; }



        public int TrainingDataSetItems { get; protected set; }
        public int TestDataSetItems { get; protected set; }
        public bool ReadTrainingDataset { get; protected set; }

        public bool ReadTestDataset { get; protected set; }

        public string ClassifierType { get; protected set; }
        public int NumberofFeatures { get; protected set; }
        public int NumberofClasses { get; protected set; }
        public int NumberofParameters { get; protected set; }
        #endregion

        #region Methods

        #endregion

        #region Fields
        protected JavaCommand javaCommand;
        private static Regex builtClassifierRegex = new Regex("Built this classifier: (\\S+) with (\\d+) features, (\\d+) classes, and (\\d+) parameters.", RegexOptions.Compiled);
        private static Regex readDataSetRegex = new Regex("Reading dataset from (.+)done \\[(\\d+\\.\\d+)s, (\\d+) items\\]", RegexOptions.Compiled);
        private static Regex classRegex = new Regex("Cls (\\S+): TP=(\\d+) FN=(\\d+) FP=(\\d+) TN=(\\d+); Acc 0.(\\d+) P 0.(\\d+) R 0.(\\d+) F1 0.(\\d+)", RegexOptions.Compiled);
        #endregion
    }
}
