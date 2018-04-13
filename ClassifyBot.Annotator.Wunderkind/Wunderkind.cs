using System;
using System.Collections.Generic;
using CommandLine;

namespace ClassifyBot
{
    public abstract class Wunderkind<TRecord, TFeature> : Annotator<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        protected override Func<Transformer<TRecord, TFeature>, Dictionary<string, object>, TRecord, TRecord> TransformInputToOutput => throw new NotImplementedException();

        protected override StageResult Init()
        {
            return base.Init();
        }

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }
        #endregion

        #region Properties
        [Value(0, Min = 1, Max = 2, Required = true, HelpText = "Enter the annotator interface: 1 = Console, 2 = Web.")]
        public int _Interface
        {   get => (int)Interface;
            set => Interface = (AnnotatorInterface)value;
        }
        #endregion

        
    }
}
