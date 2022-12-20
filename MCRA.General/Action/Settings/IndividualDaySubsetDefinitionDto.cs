using System.Collections.Generic;

namespace MCRA.General.Action.Settings {
    public class IndividualDaySubsetDefinitionDto {
        public virtual string NameIndividualProperty { get; set; }

        public virtual List<int> MonthsSubset { get; set; } = new List<int>();

        public virtual bool IncludeMissingValueRecords { get; set; }
    }
}
