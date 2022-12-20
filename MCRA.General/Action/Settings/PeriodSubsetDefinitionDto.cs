using MCRA.Utils.DateTimes;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MCRA.General.Action.Settings.Dto {
    public class PeriodSubsetDefinitionDto {

        public virtual bool AlignSampleDateSubsetWithPopulation { get; set; }

        public virtual List<string> YearsSubset { get; set; } = new List<string>();

        public virtual bool AlignSampleSeasonSubsetWithPopulation { get; set; }

        public virtual List<int> MonthsSubset { get; set; } = new List<int>();

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
