﻿namespace MCRA.General.Action.Settings {
    public class IndividualDaySubsetDefinition {
        public virtual string NameIndividualProperty { get; set; }

        public virtual List<int> MonthsSubset { get; set; } = [];

        public virtual bool IncludeMissingValueRecords { get; set; }
    }
}
