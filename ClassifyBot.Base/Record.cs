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
        public Record(string id, (string, float) label, params (string, TFeature)[] features) : this(label, features)
        {
            this.Id = id;
        }

        public Record(int id, (string, float) label, params (string, TFeature)[] features) : this(label, features)
        {
            this._Id = id;
        }

        public Record(int id, string label, params (string, TFeature)[] features) : this(id, (label, 1), features) { }

        protected Record( (string,float) label, params (string, TFeature) [] features)
        {
            this.Labels.Add(label);
            for (int i = 0; i < features.Length; i++)
            {
                this.Features.Add(features[i]);
            }
        }

        
        #endregion

        #region Properties
        public int? _Id { get; }
        public string Id { get; set; }
        public List<ValueTuple<string, float>> Labels { get; set; } = new List<(string, float)>();
        public List<ValueTuple<string, TFeature>> Features { get; set; } = new List<(string, TFeature)>();
        #endregion
    }
}
