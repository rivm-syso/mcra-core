using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class AggregateIndividualDayExposureCollection : TargetExposure {
        public List<AggregateIndividualDayExposure> AggregateIndividualDayExposures { get; set; }
    }
}
