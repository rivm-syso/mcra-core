using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class AggregateIndividualTargetExposureWrapper
        : AggregateIndividualTargetExposureWrapperBase<AggregateIndividualExposure>
    {
        public AggregateIndividualTargetExposureWrapper(
            AggregateIndividualExposure aggregateIndividualDayExposure,
            TargetUnit targetUnit
        ) : base(aggregateIndividualDayExposure, targetUnit) {
        }
    }
}
