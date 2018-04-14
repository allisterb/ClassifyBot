using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyBot
{
    public abstract class LabelAnnotator<TRecord, TFeature> : Annotator<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> 
        where TRecord : Record<TFeature> 
    {
        #region Overriden members
        protected override StageResult Read()
        {
            if (!Success(base.Read(), out StageResult r)) return r;
            RecordsToAnnotate = InputRecords.Where(record => record.Labels == null || record.Labels.Count == 0 || record.Labels.Any(l => l.Item2 == 0f)).ToList();
            Labels = InputRecords.Where(record => record.Labels != null && record.Labels.Count > 0 && record.Labels.Any(l => l.Item2 > 0))?
                .SelectMany(record => record.Labels)
                .ToList();
            if (Labels == null || Labels.Count == 0)
            {
                Error("Did not read any labels from {0} records", InputRecords.Count);
                return StageResult.FAILED;
            }
            else
            {
                Info("Read {0} labels from {1} records", Labels.Count, InputRecords.Count);
                return StageResult.SUCCESS;
                
            }
        }
        #endregion

        #region Properties
        public List<(string, float)> Labels { get; protected set; }
        public SortedList<int, string> LabelDescriptions { get; protected set; }
        #endregion
    }
}
