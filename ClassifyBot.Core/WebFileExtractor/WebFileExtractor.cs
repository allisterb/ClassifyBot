using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassifyBot
{

    public class WebFileExtractor<TRecord, TFeature> : Extractor<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public WebFileExtractor(FileInfo outputFile, bool overwrite, bool append, Dictionary<string, object> options = null) : base(outputFile, overwrite, append, options)
        { }
        #endregion

            #region Implemented methods
        public override int Extract(Dictionary<string, string> options, int? recordBatchSize = null, int? recordLimit = null)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
