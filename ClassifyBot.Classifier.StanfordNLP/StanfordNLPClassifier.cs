﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using CommandLine;

namespace ClassifyBot
{
    public abstract class StanfordNLPClassifier<TRecord, TFeature> : Classifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        public override StageResult Train(Dictionary<string, object> options = null)
        {
            ClassifierPropsFile = CreatePropsFile(ClassifierProperties);
            if (!ClassifierPropsFile.Exists)
            {
                Error("Could not find classifier props file {0}.", ClassifierPropsFile.FullName);
            }
            else
            {
                Debug("Using classifier props file {0}.", ClassifierPropsFile.FullName);
            }

            javaCommand = new JavaCommand(JavaHome, ClassPath, "edu.stanford.nlp.classify.ColumnDataClassifier", "-mx16000m", 
                "-trainFile", TrainingFile.FullName, "-testFile", TestFile.FullName, "-prop", ClassifierPropsFile.FullName);
            PrintCommand(javaCommand);
            Task c = javaCommand.Run();
            if (!CheckCommandStartedAndReport(javaCommand))
            {
                return StageResult.FAILED;
            }
 
            ClassifierOutput = new List<string>();

            foreach (string s in javaCommand.GetOutputAndErrorLines())
            {
                if (!BuiltClassifier && s.StartsWith("Built this classifier"))
                {
                    BuiltClassifier = true;
                    
                    Match m = builtClassifierRegex.Match(s);
                    if (m.Success)
                    {
                        ClassifierType = m.Groups[1].Value;
                        NumberofFeatures = Int32.Parse(m.Groups[2].Value);
                        NumberofClasses = Int32.Parse(m.Groups[3].Value);
                        NumberofParameters = Int32.Parse(m.Groups[4].Value);
                        Info("Built classifier {0} with {1} features, {2} classes and {3} parameters.", ClassifierType, NumberofFeatures, NumberofClasses, NumberofParameters);
                    }
                }
                else if (ClassifierType.IsEmpty() && s.StartsWith("QNMinimizer called on double function"))
                {
                    ClassifierType = "BinaryLogisticClassifier";
                    Match m = binaryClassiferQNN.Match(s);
                    if (m.Success)
                    {
                        NumberofFeatures = Int32.Parse(m.Groups[1].Value);
                        Info("Built classifier {0} with {1} features.", ClassifierType, NumberofFeatures);
                    }
                    else
                    {
                        Error("Could not parse BinaryLogisticClassifier output: {0}.", s);
                    }
                }

            
                else if (!ReadTrainingDataset && s.StartsWith("Reading dataset from {0} ... done".F(TrainingFile.FullName)))
                {
                    ReadTrainingDataset = true;
                    Match m = readDataSetRegex.Match(s);
                    if (m.Success)
                    {
                        TrainingDataSetItems = Int32.Parse(m.Groups[3].Value);
                        Info("{0} items in training dataset read in {1} s.", TrainingDataSetItems, m.Groups[2].Value);
                    }
                    else
                    {
                        Error("Could not parse classifier output line: {0}.", s);
                        return StageResult.FAILED;
                    }
                }

                else if (!ReadTestDataset && s.StartsWith("Reading dataset from {0} ... done".F(TestFile.FullName)))
                {
                    ReadTestDataset = true;
                    Match m = readDataSetRegex.Match(s);
                    if (m.Success)
                    {
                        TestDataSetItems = Int32.Parse(m.Groups[3].Value);
                        Info("{0} items in test dataset read in {1} s.", TestDataSetItems, m.Groups[2].Value);
                    }
                    else
                    {
                        Error("Could not parse classifier output line: {0}.", s);
                        return StageResult.FAILED;
                    }
                }

                else if (!KFoldCrossValidation && s.StartsWith("### Fold"))
                {
                    KFoldCrossValidation = true;
                    Match m = kFold.Match(s);
                    if (m.Success)
                    {
                        if (!KFoldIndex.HasValue)
                        {
                            MicroAveragedF1Folds = new float[10];
                            MacroAveragedF1Folds = new float[10];
                        }
                        KFoldIndex = Int32.Parse(m.Groups[1].Value);
                    }
                }

                else if (KFoldCrossValidation && s.StartsWith("### Fold"))
                {
                    Match m = kFold.Match(s);
                    if (m.Success)
                    {
                        KFoldIndex = Int32.Parse(m.Groups[1].Value);
                    }
                    else
                    {
                        Error("Could not parse k-fold output line: {0}.", s);
                        return StageResult.FAILED;
                    }
                }

                else if (!KFoldCrossValidation && !MicroAveragedF1.HasValue && s.StartsWith("Accuracy/micro-averaged F1"))
                {
                    Match m = f1MicroRegex.Match(s);
                    if (m.Success)
                    {
                        MicroAveragedF1 = Single.Parse(m.Groups[1].Value);
                        Info("Micro-averaged F1 = {0}.", MicroAveragedF1);
                    }
                    else
                    {
                        Error("Could not parse micro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (KFoldCrossValidation && ReadTestDataset && !MicroAveragedF1.HasValue && s.StartsWith("Accuracy/micro-averaged F1"))
                {
                    Match m = f1MicroRegex.Match(s);
                    if (m.Success)
                    {
                        MicroAveragedF1 = Single.Parse(m.Groups[1].Value);
                        Info("Micro-averaged F1 = {0}.", MicroAveragedF1);
                    }
                    else
                    {
                        Error("Could not parse micro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (KFoldCrossValidation && s.StartsWith("Accuracy/micro-averaged F1"))
                {
                    Match m = f1MicroRegex.Match(s);
                    if (m.Success)
                    {
                        MicroAveragedF1Folds[KFoldIndex.Value] = Single.Parse(m.Groups[1].Value);
                        Info("Fold {0} Micro-averaged F1 = {1}.", KFoldIndex.Value, MicroAveragedF1Folds[KFoldIndex.Value]);
                    }
                    else
                    {
                        Error("Could not parse micro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (!KFoldCrossValidation && !MacroAveragedF1.HasValue && s.StartsWith("Macro-averaged F1"))
                {
                    Match m = f1MacroRegex.Match(s);
                    if (m.Success)
                    {
                        MacroAveragedF1 = Single.Parse(m.Groups[1].Value);
                        Info("Macro-averaged F1 = {0}.", MacroAveragedF1);
                    }
                    else
                    {
                        Error("Could not parse macro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (KFoldCrossValidation && ReadTestDataset && !MacroAveragedF1.HasValue && s.StartsWith("Macro-averaged F1"))
                {
                    Match m = f1MacroRegex.Match(s);
                    if (m.Success)
                    {
                        MacroAveragedF1Folds[KFoldIndex.Value] = Single.Parse(m.Groups[1].Value);
                        Info("Macro-averaged F1 = {0}.\n", MacroAveragedF1Folds[KFoldIndex.Value]);
                    }
                    else
                    {
                        Error("Could not parse macro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (KFoldCrossValidation && s.StartsWith("Macro-averaged F1"))
                {
                    Match m = f1MacroRegex.Match(s);
                    if (m.Success)
                    {
                        MacroAveragedF1Folds[KFoldIndex.Value] = Single.Parse(m.Groups[1].Value);
                        Info("Fold {0} Macro-averaged F1 = {1}.\n", KFoldIndex.Value, MacroAveragedF1Folds[KFoldIndex.Value]);
                    }
                    else
                    {
                        Error("Could not parse macro-averaged F1 statistic {0}.", s);
                    }
                }

                else if (Features == null && s.StartsWith("Built this classifier: 1"))
                {
                    Features = new Dictionary<string, float>();
                    string f = s.Remove(0,"Built this classifier: ".Length);
                    foreach (string l in f.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] ls = l.Split('=');
                        Features.Add(ls[0].Trim(), Single.Parse(ls[1].Trim()));
                    }
                    Info("Using {0} features.", Features.Count);
          
                }
                else if (s.StartsWith("Cls"))
                {
                    Match m = classStatisticRegex.Match(s);
                    if (m.Success)
                    {
                        ClassStatistic cs = new ClassStatistic()
                        {
                            Name = m.Groups[1].Value,
                            TruePositives = Int32.Parse(m.Groups[2].Value),
                            FalsePositives = Int32.Parse(m.Groups[3].Value),
                            TrueNegatives = Int32.Parse(m.Groups[4].Value),
                            Accuracy = Single.Parse(m.Groups[5].Value),
                            Precision = Single.Parse(m.Groups[6].Value),
                            Recall = Single.Parse(m.Groups[7].Value),
                            F1 = Single.Parse(m.Groups[8].Value)
                        };
                        _ClassStatistics.Add(cs);
                        Info(s);
                    }
                    else
                    {
                        L.Error("Could not parse class statistic: {0}.", s);
                    }
                }

                else if (resultRegex.IsMatch(s))
                {
                    Match m = resultRegex.Match(s);
                    ClassifierResult cr = new ClassifierResult()
                    {
                        GoldAnswer = m.Groups[1].Value,
                        ClassifierAnswer = m.Groups[2].Value,
                        P_GoldAnswer = Single.Parse(m.Groups[3].Value),
                        P_ClAnswer = Single.Parse(m.Groups[4].Value)

                    };
                    _Results.Add(cr);
                }
                ClassifierOutput.Add(s);
                Debug(s);
            }

            c.Wait();
            if (!CheckCommandSuccessAndReport(javaCommand))
            {
                return StageResult.FAILED;
            }
            
            if (!KFoldCrossValidation)
            {
                Info("Got {0} class statistics.", _ClassStatistics.Count);
                Info("Got {0} results.", _Results.Count);
            }
            return StageResult.SUCCESS;
        }
        
        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;

            if (JavaHome.IsEmpty())
            {
                if (AdditionalOptions.ContainsKey("JAVA_HOME"))
                {
                    JavaHome = (string)AdditionalOptions["JAVA_HOME"];
                }
                else if ((JavaHome = Environment.GetEnvironmentVariable("JAVA_HOME")).IsEmpty())
                {
                    Error("The java-home or JAVA_HOME option was not specified and the JAVA_HOME environment variable does not exist.");
                    return StageResult.INVALID_OPTIONS;
                }
            }
            if (!Directory.Exists(JavaHome))
            {
                Error("The Java home directory specified does not exist: {0}.", JavaHome);
                return StageResult.INVALID_OPTIONS;
            }

            if (ClassPath.IsEmpty())
            {
                if (AdditionalOptions.ContainsKey("STANFORD_CLASSIFIER_JAR"))
                {
                    ClassPath = (string)AdditionalOptions["STANFORD_CLASSIFIER_JAR"];
                }
                else if ((ClassPath = Environment.GetEnvironmentVariable("STANFORD_CLASSIFIER_JAR")).IsEmpty())
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

            if (BinaryLogisticClassifier && !AdditionalOptions.ContainsKey("useBinary"))
            {
                AdditionalOptions.Add("useBinary", true);
            }

            if (WithKFoldCrossValidation)
            {
                ClassifierProperties.Add("crossValidationFolds", 10);
                Info("Using 10-fold cross validation");
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
           
            foreach (KeyValuePair<string, object> kv in AdditionalOptions)
            {
                if (ClassifierProperties.ContainsKey(kv.Key))
                {
                    ClassifierProperties[kv.Key] = kv.Value;
                }
                else
                {
                    ClassifierProperties.Add(kv.Key, kv.Value);
                }
                Info("Using additional classifier property {0}={1}.", kv.Key, kv.Value);
            }

            if (ClassifierProperties.ContainsKey("useNB"))
            {
                ClassifierProperties.Remove("useBinary");
            }

            return StageResult.SUCCESS;

        }

        protected override StageResult Cleanup()
        {
            if (!KeepPropsFile && ClassifierPropsFile != null && ClassifierPropsFile.Exists)
            {
                ClassifierPropsFile.Delete();
            }
            return StageResult.SUCCESS;
        }

        protected override StageResult Write() => StageResult.SUCCESS;
        protected override StageResult Process() => StageResult.SUCCESS;
        protected override StageResult Read() => StageResult.SUCCESS;
        #endregion

        #region Properties
        public virtual Dictionary<string, object> ClassifierProperties { get; } = new Dictionary<string, object>()
        {
            {"1.useSplitWords", true },
            {"1.splitWordsRegexp", "\\\\s+" },
            {"2.useSplitWords", true },
            {"2.splitWordsRegexp", "\\\\s+" },
            {"displayedColumn", -1 }
        };

        public FileInfo ClassifierPropsFile { get; protected set; }

        public List<string> ClassifierOutput { get; protected set; }
        
        public int? TrainingDataSetItems { get; protected set; }

        public int? TestDataSetItems { get; protected set; }

        public bool BuiltClassifier { get; protected set; }

        public bool ReadTrainingDataset { get; protected set; }

        public bool ReadTestDataset { get; protected set; }

        public bool KFoldCrossValidation { get; protected set; }

        public int? KFoldIndex { get; protected set; }

        public string ClassifierType { get; protected set; }

        public int? NumberofFeatures { get; protected set; }

        public int? NumberofClasses { get; protected set; }

        public Dictionary <string, float> Features { get; protected set; }

        public int? NumberofParameters { get; protected set; }

        public float? MicroAveragedF1 { get; protected set; }

        public float? MacroAveragedF1 { get; protected set; }

        public float[] MicroAveragedF1Folds { get; protected set; }

        public float[] MacroAveragedF1Folds { get; protected set; }

        [Option('j', "java-home", Required = false, HelpText = "The path to an existing Java installation. If this is not specified then the JAVA_HOME environment variable will be used")]
        public virtual string JavaHome { get; set; }

        [Option("class-path", Required = false, HelpText = "The path to the Stanford NLP Classifier jar file. If this is not specified then the JAVA_HOME environment variable will be used")]
        public virtual string ClassPath { get; set; }

        [Option('k', "keep-props-file", Required = false, Default = false, HelpText = "Don't delete the classifier properties file after classification task completes.")]
        public bool KeepPropsFile { get; set; }

        [Option('b', "binary", Required = false, Default = false, HelpText = "Use a binary logistic classifier.")]
        public bool BinaryLogisticClassifier { get; set; }

        [Option('n', "bayes", Required = false, Default = false, HelpText = "Use a Naive Bayes generative classifier.")]
        public bool NaiveBayesClassifier { get; set; }

        [Option("with-kcross", HelpText = "Use k-fold cross-validation.", Required = false)]
        public bool WithKFoldCrossValidation { get; set; }
        #endregion

        #region Methods
        protected FileInfo CreatePropsFile(Dictionary<string, object> properties)
        {
            using (TextWriter tw = File.CreateText("classifier.prop"))
            {
                foreach(KeyValuePair<string, object> kv in properties)
                {
                    tw.WriteLine("{0}={1}".F(kv.Key, kv.Value));
                }
            }
            FileInfo file = new FileInfo("classifier.prop");
            return  file;
        }
        #endregion

        #region Fields
        protected JavaCommand javaCommand;
        protected static Regex builtClassifierRegex = new Regex("Built this classifier: (\\S+) with (\\d+) features, (\\d+) classes, and (\\d+) parameters.", RegexOptions.Compiled);
        protected static Regex readDataSetRegex = new Regex("Reading dataset from (.+)done \\[(\\d+\\.\\d+)s, (\\d+) items\\]", RegexOptions.Compiled);
        protected static Regex classStatisticRegex = new Regex("Cls (\\S+): TP=(\\d+) FN=(\\d+) FP=(\\d+) TN=(\\d+); Acc (\\d+.\\d+) P (\\d+.\\d+) R (\\d+.\\d+) F1 (\\d+.\\d+)", RegexOptions.Compiled);
        protected static Regex resultRegex = new Regex("(\\S+)\\s+(\\S+)\\s+(\\d+.\\d+)\\s+(\\d+.\\d+)", RegexOptions.Compiled);
        protected static Regex f1MicroRegex = new Regex("Accuracy/micro-averaged F1: ([0|1]\\.\\d+)");
        protected static Regex f1MacroRegex = new Regex("Macro-averaged F1: ([0|1]\\.\\d+)");
        protected static Regex binaryClassiferQNN = new Regex("QNMinimizer called on double function of (\\d+) variables");
        protected static Regex kFold = new Regex("### Fold (\\d+)");
        #endregion
    }
}
