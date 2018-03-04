using System;


namespace ClassifyBot
{
    public interface IRecord<T> where T : ICloneable, IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {
        string Label { get; }
        T[] Features { get; }
    }
}
