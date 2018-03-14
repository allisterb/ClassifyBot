using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassifyBot
{
    public interface IRecord<TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature>
    {
        string Id { get; set; }
        List<ValueTuple<string, float>> Labels { get; set; }
        List<ValueTuple<string, TFeature>> Features { get; set; }
        
    }
}
