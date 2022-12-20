using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundRPFDataSection : SummarySection {

        public List<CompoundRPFDataRecord> Records { get; set; }
    }
}
