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
            Contract.Requires(!TrainingFileName.Empty());
            Contract.Requires(!TestFileName.Empty());
            Contract.Requires(!ModelFileName.Empty());
        }
        #endregion

        #region Properties
        public FileInfo TrainingFile { get; protected set; }

        public FileInfo TestFile { get; protected set; }

        public FileInfo ModelFile { get; protected set; }

        [Option('t', "training-file", Required = true, HelpText = "Input file name with training data for classifier")]
        public string TrainingFileName { get; set; }

        [Option('e', "training-file", Required = true, HelpText = "Input file name with test data for classifier")]
        public string TestFileName { get; set; }

        [Option('m', "model-file", Required = true, HelpText = "Output file name for classifier model.")]
        public string ModelFileName { get; set; }

        [Option("train", HelpText = "Train a classifier model using the training and test data files.", SetName = "op")]
        public bool TrainOp { get; set; }
        #endregion

        #region Abstract members
        public abstract StageResult Train(Dictionary<string, string> options = null);
        #endregion
    }
}
