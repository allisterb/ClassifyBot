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
    public abstract class LoadToCsvFile<TRecord, TFeature> : Loader<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public LoadToCsvFile(string delimiter = ",") : base()
        {
            WriterOptions.Add("Delimiter", delimiter);   
        }
        #endregion

        #region Overriden members
        protected override Func<ILogger, StreamWriter, List<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (logger, sw, records, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                csv.Configuration.HasHeaderRecord = false;
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                for (int i = 0; i < records.Count(); i++)
                {
                    csv.WriteField(records[i].Labels[0].Item1);
                    for (int f = 0; f < records[i].Features.Count; f++)
                    {
                        csv.WriteField(records[i].Features[f].Item2);
                    }
                    csv.NextRecord();
                }
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
                    foreach (KeyValuePair<string, object> kv in AdditionalOptions)
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
