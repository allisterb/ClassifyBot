using System;
using System.Collections.Generic;
using CommandLine;

using ClassifyBot.Annotator.Wunderkind;

namespace ClassifyBot
{
    public abstract class WunderkindLabelAnnotator<TRecord, TFeature> : LabelAnnotator<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        protected override Func<Transformer<TRecord, TFeature>, Dictionary<string, object>, TRecord, TRecord> TransformInputToOutput => throw new NotImplementedException();

        protected override StageResult Init()
        {
            if (!Success(base.Init(), out StageResult r)) return r;
            switch(InterfaceType)
            {
                case AnnotatorInterfaceType.Console:
                    AnnotatorInterface = new LabelAnnotatorApp<TRecord, TFeature>(this);
                    return StageResult.SUCCESS;
                default:
                    Error("Unsupported interface type requested: {0}.", InterfaceType);
                    return StageResult.FAILED;
            }
        }

        protected override StageResult Cleanup()
        {
            return StageResult.SUCCESS;
        }
        #endregion

        #region Properties
        [Value(0, Required = true, HelpText = "Enter the annotator interface: 1 = Console, 2 = Web.")]
        public int _InterfaceType
        {   get => (int)InterfaceType;
            set => InterfaceType = (AnnotatorInterfaceType)value;
        }
        #endregion
    }
}
