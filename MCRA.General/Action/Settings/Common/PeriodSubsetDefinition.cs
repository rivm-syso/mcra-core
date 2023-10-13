using MCRA.Utils.DateTimes;
using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {
    public class PeriodSubsetDefinition {

        public virtual bool AlignSampleDateSubsetWithPopulation { get; set; }

        public virtual List<string> YearsSubset { get; set; } = new();

        public virtual bool AlignSampleSeasonSubsetWithPopulation { get; set; }

        public virtual List<int> MonthsSubset { get; set; } = new();

        public virtual bool IncludeMissingValueRecords { get; set; }

        [XmlIgnore]
        public List<TimeRange> YearsSubsetTimeRanges {
            get {
                return YearsSubset?
                    .Select(s => s != null ? new TimeRange(int.Parse(s)) : null)
                    .ToList();
            }
        }
    }
}
