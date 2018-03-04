using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClassifyBot
{
    public class Record<T> : IRecord<T> where T : ICloneable, IComparable, IComparable<T>, IConvertible, IEquatable<T>
    {
        public int? Id { get; }
        public string Label { get; }
        public T[] Features { get; }
    }
}
