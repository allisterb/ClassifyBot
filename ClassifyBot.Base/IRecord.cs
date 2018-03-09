using System;
using System.Collections.Generic;

namespace ClassifyBot
{
    public interface IRecord<T> where T : ICloneable, IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {
        string Label { get; }
        List<T> Features { get; }
    }
}
