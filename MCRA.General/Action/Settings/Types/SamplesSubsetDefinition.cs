﻿namespace MCRA.General.Action.Settings {

    public class SamplesSubsetDefinition {

        public virtual string PropertyName { get; set; }

        public virtual bool AlignSubsetWithPopulation { get; set; }

        public virtual HashSet<string> KeyWords { get; set; }

        public virtual bool IncludeMissingValueRecords { get; set; }

        public bool IsRegionSubset => string.Equals(PropertyName, "Region", StringComparison.OrdinalIgnoreCase);

        public bool IsProductionMethodSubset => string.Equals(PropertyName, "ProductionMethod", StringComparison.OrdinalIgnoreCase);
    }
}
