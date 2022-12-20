using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TestSystemsSummarySection : SummarySection {
        public List<TestSystemsSummaryRecord> Records { get; set; }
    }
}
