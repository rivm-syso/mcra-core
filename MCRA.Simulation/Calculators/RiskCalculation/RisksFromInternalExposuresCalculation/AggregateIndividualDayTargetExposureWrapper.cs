using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public sealed class AggregateIndividualDayTargetExposureWrapper
        : AggregateIndividualTargetExposureWrapperBase<AggregateIndividualDayExposure>
          , ITargetIndividualDayExposure
    {
        public AggregateIndividualDayTargetExposureWrapper(
            AggregateIndividualDayExposure aggregateIndividualDayExposure,
            TargetUnit targetUnit
        ) : base(aggregateIndividualDayExposure, targetUnit) {
        }

        public string Day => _aggregateIndividualExposure.Day;

        public int SimulatedIndividualDayId =>
            _aggregateIndividualExposure.SimulatedIndividualDayId;
    }
}
