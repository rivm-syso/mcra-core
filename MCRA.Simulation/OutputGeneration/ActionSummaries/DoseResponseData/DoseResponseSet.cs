using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseSet {
        public string ModelCode { get; set; }
        public string SubstanceCode { get; set; }
        public string SubstanceName { get; set; }
        public string CovariateLevel { get; set; }
        public double RPF { get; set; } = 1;
        public DoseResponseRecord CriticalEffectDoseResponseRecord { get; set; }
        public List<DoseResponseRecord> DoseResponseRecords { get; set; }
    }
}
