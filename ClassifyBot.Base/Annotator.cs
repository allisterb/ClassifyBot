using System;
using System.Collections.Generic;
using System.Text;

using Serilog;
using SerilogTimings;

namespace ClassifyBot
{
    public enum AnnotatorInterfaceType
    {
        None = 0,
        Console = 1,
        Web = 2
    }

    public abstract class Annotator<TRecord, TFeature> : Transformer<TRecord, TFeature>, IAnnotator<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Overriden members
        protected override StageResult Process()
        {
            if (!Success(AnnotatorInterface.Init(), out StageResult r)) return r;
            return AnnotatorInterface.Run();
        }
        #endregion

        #region Properties
        public List<TRecord> RecordsToAnnotate { get; protected set; }
        public AnnotatorInterfaceType InterfaceType { get; protected set; }
        public Dictionary<string, object> InterfaceOptions { get; set; } = new Dictionary<string, object>();
        public IAnnotatorInterface<TRecord, TFeature> AnnotatorInterface { get; protected set; }
        #endregion
    }
}
