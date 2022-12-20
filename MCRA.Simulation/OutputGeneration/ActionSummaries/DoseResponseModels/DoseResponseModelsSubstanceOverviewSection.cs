using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseModelsSubstanceOverviewSection : SummarySection {
        public List<DoseResponseModelSubstanceSummaryRecord> SummaryRecords { get; set; }
    }
}
