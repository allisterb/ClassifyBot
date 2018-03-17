using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using System.Threading.Tasks;
using CommandLine;

namespace ClassifyBot
{
    public abstract class StanfordNLPClassifier<TRecord, TFeature> : Classifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        public override StageResult Train(Dictionary<string, string> options = null)
        {
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
                Error("The Java Home directory specified does not exist: {0}.", JavaHome);
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
            if (!version.CommandStarted)
            {
                Error("Could not detect Java version.");
                return StageResult.FAILED;
            }
            else
            {
                c.Wait();
                if (c.IsCompleted && version.Success)
                {
                    Info(version.StandardError);
                }
                if (c.IsCompleted && !version.Success)
                {
                    Error("Could not detect Java version: {0}", version.StandardError);
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
        #endregion

        #region Fields
        protected JavaCommand javaCommand;
        #endregion
    }
}
