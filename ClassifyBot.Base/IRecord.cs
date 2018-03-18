using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassifyBot
{
    public interface IRecord<TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature>
    {
        int? _Id { get; }
        string Id { get; }
        List<ValueTuple<string, float>> Labels { get; }
        List<ValueTuple<string, TFeature>> Features { get; }
        
    }
}
