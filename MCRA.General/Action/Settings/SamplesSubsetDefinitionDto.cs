using System;
using System.Collections.Generic;

namespace MCRA.General.Action.Settings.Dto {

    public class SamplesSubsetDefinitionDto {

        public virtual string PropertyName { get; set; }

        public virtual bool AlignSubsetWithPopulation { get; set; }

        public virtual HashSet<string> KeyWords { get; set; }

        public virtual bool IncludeMissingValueRecords { get; set; }

        public bool IsRegionSubset() {
            return string.Equals(PropertyName, "Region", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsProductionMethodSubset() {
            return string.Equals(PropertyName, "ProductionMethod", StringComparison.OrdinalIgnoreCase);
        }
    }
}
