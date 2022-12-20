using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {

    public class SingleValueRiskCalculationResult {
        public IExposureSource Source { get; set; }
        public Compound Substance { get; set; }
        public double Exposure { get; set; }
        public double HazardCharacterisation { get; set; }
        public double HazardQuotient { get; set; }
        public double MarginOfExposure { get; set; }
        public PotencyOrigin Origin { get; set; }
    }
}
