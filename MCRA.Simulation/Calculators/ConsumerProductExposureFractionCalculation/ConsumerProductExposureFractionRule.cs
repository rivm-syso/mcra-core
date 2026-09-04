using MCRA.General;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureFractionCalculation {
    public class ConsumerProductExposureFractionRule {
        public double ExposureFraction { get; set; }
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
    }
}
