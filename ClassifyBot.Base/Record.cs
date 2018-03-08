using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClassifyBot
{
    public class Record<TFeature> : IRecord<TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature>
    {
        #region Constructors
        public Record(int id, string label, params TFeature[] features) : this(label, features)
        {
            this.Id = id;
        }
        public Record(string label, params TFeature[] features)
        {
            this.Label = label;
            this.Features = features;
        }

        #endregion
        
        #region Properties
        public int? Id { get; set; }
        public string Label { get; set; }
        public TFeature[] Features { get; set; }
        #endregion
    }
}
