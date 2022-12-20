using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ResponseSummarySection : SummarySection {
        public List<ResponseSummaryRecord> Records { get; set; }
    }
}
