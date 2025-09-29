using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation {
    public class KineticConversionFactorResultRecord {
        public Compound Substance { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public ExposureUnitTriple ExternalExposureUnit { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public double Factor { get; set; }
    }
}
