using System;
using System.Collections.Generic;
using System.IO;


namespace ClassifyBot
{
    public interface IClassifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        StageResult Train(Dictionary<string, object> options = null);
        FileInfo TrainingFile { get; }
        FileInfo TestFile { get; }
        FileInfo ModelFile { get; }
        IEnumerable<IClassStatistic> ClassStatistics { get;  }
        IEnumerable<IClassifierResult> Results { get; }
    }
}
