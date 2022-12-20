using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PointsOfDepartureSummarySection : SummarySection {
        public List<PointsOfDepartureSummaryRecord> Records { get; set; }
    }
}
