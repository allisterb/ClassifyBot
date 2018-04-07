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
            WriterOptions.Add("HasHeaderRecord", false);
            WriterOptions.Add("QuoteNoFields", true);
        }
        #endregion

        #region Overriden members
        protected override Func<Loader<TRecord, TFeature>, StreamWriter, List<TRecord>, Dictionary<string, object>, StageResult> WriteFileStream { get; } = (l, sw, records, options) =>
        {
            using (CsvWriter csv = new CsvWriter(sw))
            {
                SetPropFromDict(csv.Configuration.GetType(), csv.Configuration, options);
                for (int i = 0; i < records.Count; i++)
                {
                    TRecord record = records[i];
                    for (int j = 0; j < record.Labels.Count; j++)
                    {
                        csv.WriteField(record.Labels[j].Item1.ToUpper());
                    }

                    for (int f = 0; f < record.Features.Count; f++)
                    {
                        csv.WriteField(record.Features[f].Item2.ToString()
                            .Replace("\r", string.Empty)
                            .Replace("\n", string.Empty)
                            .Replace("\r\n", string.Empty)
                            .Replace("\t", string.Empty));
                    }
                    if (!record.Id.IsEmpty())
                    {
                        csv.WriteField(record.Id);
                    }
                    else if (record._Id.HasValue)
                    {
                        csv.WriteField(record._Id);
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

            if (AdditionalOptions.Count > 0)
            {
                foreach (KeyValuePair<string, object> kv in AdditionalOptions)
                {
                    if (WriterOptions.ContainsKey(kv.Key))
                    {
                        WriterOptions.Remove(kv.Key);
                    }
                    WriterOptions.Add(kv.Key, kv.Value);
                }
            }
            return StageResult.SUCCESS;

        }

        protected override StageResult Cleanup() => StageResult.SUCCESS;
        #endregion
    }
}
