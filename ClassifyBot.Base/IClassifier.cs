using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassifyBot
{
    public interface IClassifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        StageResult Load(Func<int> split, Dictionary<string, string> options = null);
        StageResult Train(Dictionary<string, string> options = null);
        
        FileInfo TrainingFile { get; }
        FileInfo TestFile { get; }
        FileInfo ModelFile { get; }
    }
}
