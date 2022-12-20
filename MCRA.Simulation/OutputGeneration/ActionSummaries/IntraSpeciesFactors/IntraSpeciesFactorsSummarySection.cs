using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IntraSpeciesFactorsSummarySection : SummarySection {
        public List<IntraSpeciesFactorsSummaryRecord> Records { get; set; }
        public double DefaultIntraSpeciesFactor { get; set; }
    }
}
