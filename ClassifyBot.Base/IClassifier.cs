using System;
using System.Collections.Generic;
using System.IO;


namespace ClassifyBot
{
    public interface IClassifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        StageResult Train(Dictionary<string, string> options = null);
        FileInfo TrainingFile { get; }
        FileInfo TestFile { get; }
        FileInfo ModelFile { get; }
    }
}
