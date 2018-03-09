using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CsvHelper;
using Serilog;

namespace ClassifyBot
{
    public abstract class TransformToCsvFile<TRecord, TFeature> : Transformer<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public TransformToCsvFile(string delimiter = ",") : base()
        {
            
        }
        #endregion

        #region Overriden members
        public override StageResult Transform(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {

            OutputRecords = InputRecords;
            return StageResult.SUCCESS;
        }


        protected override Func<ILogger, StreamWriter, IEnumerable<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (logger, sw, records, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                csv.WriteRecords(records.Select(r => new { label = r.Label, text = r.Features[0] }));
                csv.Flush();
                return StageResult.SUCCESS;
            }

        };

        protected override StageResult Init()
        {
            StageResult r;
            if ((r = base.Init()) != StageResult.SUCCESS)
            {
                return r;
            }
            else
            {
                if (AdditionalOptions.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kv in AdditionalOptions)
                    {
                        WriterOptions.Add(kv.Key, kv.Value);
                    }
                }
                return StageResult.SUCCESS;
            }
        }
        #endregion

        #region Properties

        #endregion
    }
}
