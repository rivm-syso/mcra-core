using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class AggregateIndividualExposureCollection : TargetExposure {
        public List<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }
    }
}
