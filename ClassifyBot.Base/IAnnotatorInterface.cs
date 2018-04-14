using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public interface IAnnotatorInterface<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        StageResult Init();
        StageResult Run();
    }
}
