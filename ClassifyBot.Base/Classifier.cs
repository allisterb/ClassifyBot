using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Serilog;
using CommandLine;

namespace ClassifyBot
{
    public abstract class Classifier<TRecord, TFeature> : Stage, IClassifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public Classifier() : base()
        {
            Contract.Requires(!TrainingFileName.IsEmpty());
            Contract.Requires(!TestFileName.IsEmpty());
            Contract.Requires(!ModelFileName.IsEmpty());
        }
        #endregion

        #region Abstract members
        public abstract StageResult Train(Dictionary<string, object> options = null);
        #endregion

        #region Overidden members
        public override StageResult Run(Dictionary<string, object> options)
        {
            StageResult r;
            if ((r = Init()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Train()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Write()) != StageResult.SUCCESS)
            {
                return r;
            }
            Cleanup();
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            if (TrainOp)
            {
                Contract.Requires(TrainingFile != null && TestFile == null);
                if (!TrainingFile.CheckExistsAndReportError(L))
                {
                    return StageResult.INPUT_ERROR;
                }
                if (!TestFile.CheckExistsAndReportError(L))
                {
                    return StageResult.INPUT_ERROR;
                }
                if (!ModelFileName.IsEmpty() && ModelFile.Exists && !OverwriteOutputFile)
                {
                    Error("The model file {0} exists but the overwrite option was not specified.", ModelFile.FullName);
                    return StageResult.INPUT_ERROR;
                }
            }
            return StageResult.SUCCESS;
        }

        [Option("compress", Hidden = true)]
        public override bool CompressOutputFile { get; set; }
        #endregion

        #region Properties
        public FileInfo TrainingFile => TrainingFileName.IsEmpty() ? null : new FileInfo(TrainingFileName);

        public FileInfo TestFile => TestFileName.IsEmpty() ? null : new FileInfo(TestFileName);

        public FileInfo ModelFile => ModelFileName.IsEmpty() ? null : new FileInfo(ModelFileName);

        public IEnumerable<IClassStatistic> ClassStatistics => _ClassStatistics;

        public IEnumerable<IClassifierResult> Results => _Results;

        [Option('t', "train-file", Required = true, HelpText = "Input file name with training data for classifier.")]
        public string TrainingFileName { get; set; }

        [Option('e', "test-file", Required = true, HelpText = "Input file name with test data for classifier.")]
        public string TestFileName { get; set; }

        [Option('m', "model-file", Required = false, HelpText = "Output file name for classifier model.")]
        public string ModelFileName { get; set; }

        [Option("train", HelpText = "Train a classifier model using the training and test data files.", SetName = "op")]
        public bool TrainOp { get; set; }

        protected List<ClassStatistic> _ClassStatistics = new List<ClassStatistic>();

        protected List<ClassifierResult> _Results = new List<ClassifierResult>();
        #endregion
    }
}
