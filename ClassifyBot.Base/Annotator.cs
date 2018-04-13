using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public enum AnnotatorInterface
    {
        None = 0,
        Console = 1,
        Web = 2
    }

    public abstract class Annotator<TRecord, TFeature> : Transformer<TRecord, TFeature>, IAnnotator<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        public List<TRecord> RecordsToAnnotate { get; protected set; }
        public AnnotatorInterface Interface { get; protected set; }
    }
}
