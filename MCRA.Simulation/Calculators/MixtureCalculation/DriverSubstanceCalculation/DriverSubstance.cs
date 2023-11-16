using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation {
    public sealed class DriverSubstance {
        public Compound Substance { get; set; }
        public double MaximumCumulativeRatio { get; set; }
        public double CumulativeExposure { get; set; }
        public ExposureTarget Target { get; set; }
    }
}
