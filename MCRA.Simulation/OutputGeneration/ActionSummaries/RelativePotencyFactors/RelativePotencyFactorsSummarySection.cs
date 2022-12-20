using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RelativePotencyFactorsSummarySection : ActionSummaryBase {
        public List<RelativePotencyFactorsSummaryRecord> Records { get; set; }
    }
}
