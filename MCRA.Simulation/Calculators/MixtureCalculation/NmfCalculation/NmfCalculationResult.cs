using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation {
    public sealed class NmfCalculationResult {
        public List<ComponentRecord> ComponentRecords { get; set; }
        public GeneralMatrix SweepW { get; set; }
    }
}
